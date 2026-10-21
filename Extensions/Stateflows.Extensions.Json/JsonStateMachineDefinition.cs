using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Newtonsoft.Json.Linq;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Json;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Builders;

namespace Stateflows.StateMachines.Json;

/// <summary>A schema-validated document. CLR binding occurs before registration constructs a graph.</summary>
public sealed class JsonStateMachineDefinition
{
    private static readonly Lazy<JsonSchema> Schema = new(() =>
    {
        using var stream = typeof(JsonStateMachineDefinition).Assembly.GetManifestResourceStream(
            "Stateflows.Extensions.Json.state-machine.schema.json")!;
        using var reader = new StreamReader(stream);
        return JsonSchema.FromText(reader.ReadToEnd());
    });

    private readonly JObject document;
    private readonly IReadOnlyDictionary<string, Type> types;

    public ModelValidationReport ValidationReport { get; private set; } = new();

    private JsonStateMachineDefinition(JObject document, IReadOnlyDictionary<string, Type> types)
    {
        this.document = document;
        this.types = types;
    }

    /// <summary>Parses strict JSON, rejecting duplicate properties, and evaluates the embedded Draft 2020-12 schema.</summary>
    public static JsonStateMachineDefinition Parse(string json, JsonOptions options)
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
            using var reader = new Newtonsoft.Json.JsonTextReader(text)
            {
                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,
                MaxDepth = 64
            };
            document = JObject.Load(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
        }
        catch (System.Text.Json.JsonException exception)
        {
            report.Add("JSON-001", exception.Path ?? "/", exception.Message);
            throw new JsonStateMachineException(report, "<document>", exception);
        }
        return new JsonStateMachineDefinition(document, options.Snapshot());
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
        if (report.HasErrors) throw new JsonStateMachineException(report, name);
    }

    /// <summary>Binds all authored CLR capabilities, expands definitions, validates, then registers the model.</summary>
    public ModelValidationReport Register(IStateMachinesBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (builder is not StateMachinesBuilder stateMachines)
            throw new ArgumentException("JSON registration requires the Stateflows state-machine builder.", nameof(builder));
        ValidationReport = new ModelValidationReport();
        var bound = new BoundDocument(document, types, stateMachines.StateflowsBuilder, ValidationReport);
        bound.Bind();
        ThrowIfErrors(ValidationReport, bound.Name);
        Graph? graph = null;
        try
        {
            builder.AddStateMachine(bound.Name, bound.Version, b =>
            {
                graph = ((StateMachineBuilder)b).Graph;
                bound.Construct((StateMachineBuilder)b);
            });
            CopyDiagnostics(graph!.ValidationReport);
        }
        catch (JsonStateMachineException)
        {
            if (graph != null) CopyDiagnostics(graph.ValidationReport);
            throw;
        }
        catch (StateflowsDefinitionException exception)
        {
            if (graph?.ValidationReport.Diagnostics.Count > 0)
                CopyDiagnostics(graph.ValidationReport);
            else if (!ValidationReport.HasErrors)
                ValidationReport.Add("JSON-005", "/", exception.Message);
            throw new JsonStateMachineException(ValidationReport, bound.Name, exception);
        }
        catch (InvalidOperationException exception) when (graph?.ValidationReport.HasErrors == true)
        {
            CopyDiagnostics(graph.ValidationReport);
            throw new JsonStateMachineException(ValidationReport, bound.Name, exception);
        }
        return ValidationReport;
    }

    private void CopyDiagnostics(ModelValidationReport report)
    {
        foreach (var diagnostic in report.Diagnostics)
            if (!ValidationReport.Diagnostics.Contains(diagnostic))
                ValidationReport.Add(diagnostic.RuleId, diagnostic.ElementPath, diagnostic.Message, diagnostic.Severity);
    }
}

public static class JsonStateMachineExtensions
{
    /// <summary>Loads and registers a JSON state machine. Use Parse/Register to retain its warning report.</summary>
    public static IStateMachinesBuilder AddStateMachineJson(this IStateMachinesBuilder builder, string json,
        JsonOptions options)
    {
        JsonStateMachineDefinition.Parse(json, options).Register(builder);
        return builder;
    }
}
