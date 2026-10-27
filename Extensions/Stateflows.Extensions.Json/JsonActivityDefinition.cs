using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Json;
using Stateflows.Common.Validation;

namespace Stateflows.Activities.Json;

/// <summary>A schema-validated activity document. CLR binding occurs before builder registration.</summary>
public sealed class JsonActivityDefinition
{
    private static readonly Lazy<JsonSchema> Schema = new(() =>
    {
        using var stream = typeof(JsonActivityDefinition).Assembly.GetManifestResourceStream(
            "Stateflows.Extensions.Json.activity.schema.json")!;
        using var reader = new StreamReader(stream);
        return JsonSchema.FromText(reader.ReadToEnd());
    });

    private readonly JObject document;
    private readonly IReadOnlyDictionary<string, Type> types;

    /// <summary>Gets the most recent model-validation diagnostics produced while registering this definition.</summary>
    public ModelValidationReport ValidationReport { get; private set; } = new();

    private JsonActivityDefinition(JObject document, IReadOnlyDictionary<string, Type> types)
    {
        this.document = document;
        this.types = types;
    }

    /// <summary>Parses strict JSON and validates it against the embedded activity schema.</summary>
    /// <param name="json">The activity definition document.</param>
    /// <param name="options">Host-owned CLR type and validator aliases available to the document.</param>
    /// <returns>A parsed definition ready to bind and register.</returns>
    /// <exception cref="JsonActivityException">The JSON is malformed, contains duplicate properties, or violates the activity schema.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    public static JsonActivityDefinition Parse(string json, JsonOptions options)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(options);
        var report = new ModelValidationReport();
        JObject document;
        try
        {
            using var parsed = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64 });
            CheckDuplicates(parsed.RootElement, "", report);
            ThrowIfErrors(report, "<document>");
            var result = Schema.Value.Evaluate(JsonNode.Parse(json),
                new EvaluationOptions { OutputFormat = OutputFormat.List });
            if (!result.IsValid) AddSchemaErrors(result, report);
            ThrowIfErrors(report, "<document>");
            using var text = new StringReader(json);
            using var reader = new JsonTextReader(text)
            {
                DateParseHandling = DateParseHandling.None,
                MaxDepth = 64
            };
            document = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
        }
        catch (System.Text.Json.JsonException exception)
        {
            report.Add("JSON-001", exception.Path ?? "/", exception.Message);
            throw new JsonActivityException(report, "<document>", exception);
        }
        return new JsonActivityDefinition(document, options.Snapshot());
    }

    private static void CheckDuplicates(JsonElement value, string path, ModelValidationReport report)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                var childPath = path + "/" + Escape(property.Name);
                if (!names.Add(property.Name))
                    report.Add("JSON-001", childPath, "Duplicate JSON property.");
                CheckDuplicates(property.Value, childPath, report);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in value.EnumerateArray())
                CheckDuplicates(item, path + "/" + index++, report);
        }
    }

    private static void AddSchemaErrors(EvaluationResults result, ModelValidationReport report)
    {
        if (result.Errors != null)
            foreach (var error in result.Errors)
                report.Add("JSON-002", result.InstanceLocation.ToString() is { Length: > 0 } location ? location : "/",
                    $"{error.Key}: {error.Value}");
        if (result.HasDetails)
            foreach (var detail in result.Details.Where(d => !d.IsValid))
                AddSchemaErrors(detail, report);
    }

    internal static string Escape(string text) => text.Replace("~", "~0").Replace("/", "~1");

    internal static void ThrowIfErrors(ModelValidationReport report, string name)
    {
        if (report.HasErrors) throw new JsonActivityException(report, name);
    }

    /// <summary>Binds the document to host capabilities, validates it, and registers the activity.</summary>
    /// <param name="builder">The Stateflows activity registration builder.</param>
    /// <returns>The model validation report collected during binding and registration.</returns>
    /// <exception cref="JsonActivityException">Binding or activity graph validation fails; inspect <see cref="ValidationReport"/> for diagnostics.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="builder"/> is not the Stateflows activity builder implementation.</exception>
    public ModelValidationReport Register(IActivitiesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not ActivitiesBuilder activities)
            throw new ArgumentException("JSON registration requires the Stateflows activity builder.", nameof(builder));
        ValidationReport = new ModelValidationReport();
        var bound = new BoundActivityDocument(document, types, activities.StateflowsBuilder, ValidationReport);
        bound.Bind();
        ThrowIfErrors(ValidationReport, bound.Name);
        try
        {
            builder.AddActivity(bound.Name, bound.Version, activityBuilder => bound.Construct(activityBuilder));
        }
        catch (JsonActivityException)
        {
            throw;
        }
        catch (StateflowsDefinitionException exception)
        {
            if (!ValidationReport.HasErrors)
                ValidationReport.Add("JSON-005", "/", exception.Message);
            throw new JsonActivityException(ValidationReport, bound.Name, exception);
        }
        catch (InvalidOperationException exception)
        {
            if (!ValidationReport.HasErrors)
                ValidationReport.Add("JSON-005", "/", exception.Message);
            throw new JsonActivityException(ValidationReport, bound.Name, exception);
        }
        return ValidationReport;
    }
}

/// <summary>Convenience extensions for registering JSON-authored activities.</summary>
public static class JsonActivityExtensions
{
    /// <summary>Parses, validates, and registers an activity from JSON.</summary>
    /// <param name="builder">The Stateflows activity registration builder.</param>
    /// <param name="json">The activity definition document.</param>
    /// <param name="options">Host-owned CLR type and validator aliases available to the document.</param>
    /// <returns>The same activity builder, for further registrations.</returns>
    /// <remarks>Use <see cref="JsonActivityDefinition.Parse(string, JsonOptions)"/> and <see cref="JsonActivityDefinition.Register(IActivitiesBuilder)"/> when the resulting validation report must be retained.</remarks>
    public static IActivitiesBuilder AddActivityJson(this IActivitiesBuilder builder, string json, JsonOptions options)
    {
        JsonActivityDefinition.Parse(json, options).Register(builder);
        return builder;
    }
}
