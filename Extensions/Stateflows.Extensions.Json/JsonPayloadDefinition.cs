using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Stateflows.Common;
using Stateflows.Common.Json;

namespace Stateflows.Payloads.Json;

/// <summary>A strict payload definition bound to host-owned CLR and validation aliases.</summary>
/// <remarks>Definitions cannot load CLR assemblies. Event fields, responses, and custom validators are resolved only from explicitly registered aliases.</remarks>
public sealed class JsonPayloadDefinition
{
    private readonly EventDocument document;
    private readonly Type? responseType;

    private JsonPayloadDefinition(EventDocument document, Type? responseType)
    {
        this.document = document;
        this.responseType = responseType;
    }

    /// <summary>Parses and binds a payload document.</summary>
    /// <param name="json">The payload definition document.</param>
    /// <param name="options">The host-owned aliases available to the definition.</param>
    /// <returns>A validated definition that can be registered with an event builder.</returns>
    /// <exception cref="JsonException">The document is malformed, contains duplicate properties, or has invalid fields, validators, or response metadata.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public static JsonPayloadDefinition Parse(string json, JsonOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        using var parsed = PayloadJson.Parse(json);
        var document = PayloadJson.Read<EventDocument>(parsed.RootElement);
        PayloadJson.Require(document.FormatVersion == "1.0", "formatVersion must be '1.0'.");
        PayloadJson.Require(!string.IsNullOrWhiteSpace(document.Name), "An event name is required.");
        BindFields(document.Fields, options.ValidatorSnapshot());
        Type? responseType = null;
        if (document.Response is { } response)
        {
            PayloadJson.Require((response.EventName != null) != (response.ClrType != null),
                "response requires exactly one of eventName or clrType.");
            if (response.EventName != null)
                PayloadJson.Require(!string.IsNullOrWhiteSpace(response.EventName), "response.eventName cannot be empty.");
            else
                PayloadJson.Require(options.Snapshot().TryGetValue(response.ClrType!, out responseType),
                    $"Response CLR alias '{response.ClrType}' is not registered.");
        }
        return new JsonPayloadDefinition(document, responseType);
    }

    /// <summary>Registers this event or request definition through the core fluent event API.</summary>
    /// <param name="builder">The event registration builder.</param>
    /// <returns>The event builder returned by the underlying registration operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>Named payload responses are resolved by the core event registry; CLR response types use their registered alias.</remarks>
    public IPayloadsBuilder Register(IPayloadsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        PayloadBuildAction action = b => AddFields(b, document.Fields!);
        if (responseType != null)
        {
            var method = typeof(IPayloadsBuilder).GetMethods().Single(m => m.Name == nameof(IPayloadsBuilder.AddRequest) && m.IsGenericMethodDefinition);
            try { method.MakeGenericMethod(responseType).Invoke(builder, [document.Name!, action]); }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            }
            return builder;
        }
        return document.Response is { EventName: { } name }
            ? builder.AddRequest(document.Name!, name, action)
            : builder.AddPayload(document.Name!, action);
    }

    private static void BindFields(List<FieldDocument>? fields,
        IReadOnlyDictionary<string, Func<JsonElement, ValidationAttribute>> validators)
    {
        PayloadJson.Require(fields != null, "fields must be an array (empty is allowed).");
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in fields!)
        {
            PayloadJson.Require(field != null, "A field must be an object.");
            PayloadJson.Require(!string.IsNullOrWhiteSpace(field!.Name) && names.Add(field.Name),
                $"Field name '{field.Name}' is empty or duplicated.");
            if (field.Type == null)
            {
                BindFields(field.Fields, validators);
                PayloadJson.Require(field.ItemValidation == null, "Object fields do not support itemValidation.");
            }
            else
            {
                PayloadJson.Require(Enum.IsDefined(field.Type.Value), "Unknown field type.");
                PayloadJson.Require(field.Fields == null, "Scalar fields cannot declare fields.");
                PayloadJson.Require(field.IsCollection || field.ItemValidation == null,
                    "itemValidation requires a scalar collection.");
            }
            if (field.Type == FieldType.Enum)
                PayloadJson.Require(field.EnumValues is { Count: > 0 } &&
                    field.EnumValues.All(v => !string.IsNullOrWhiteSpace(v)) &&
                    field.EnumValues.Distinct(StringComparer.Ordinal).Count() == field.EnumValues.Count,
                    "Enum fields require distinct, nonempty enumValues.");
            else
                PayloadJson.Require(field.EnumValues == null, "enumValues requires type Enum.");
            field.Attributes = BindValidation(field.Validation, validators);
            field.ItemAttributes = BindValidation(field.ItemValidation, validators);
        }
        foreach (var field in fields!)
            foreach (var compare in field.Attributes.Concat(field.ItemAttributes).OfType<CompareAttribute>())
                PayloadJson.Require(names.Contains(compare.OtherProperty),
                    $"Compare references undeclared sibling field '{compare.OtherProperty}'.");
    }

    private static ValidationAttribute[] BindValidation(List<ValidatorDocument>? definitions,
        IReadOnlyDictionary<string, Func<JsonElement, ValidationAttribute>> validators)
        => definitions?.Select(v =>
        {
            PayloadJson.Require(v != null && !string.IsNullOrWhiteSpace(v.Type), "Validation type is required.");
            PayloadJson.Require(v!.Options.ValueKind is JsonValueKind.Undefined or JsonValueKind.Object,
                "Validation options must be an object.");
            var options = v.Options.ValueKind == JsonValueKind.Undefined
                ? JsonSerializer.SerializeToElement(new { }) : v.Options;
            ValidationAttribute attribute;
            if (v.Type == "Custom")
            {
                PayloadJson.Require(v.Alias != null && validators.ContainsKey(v.Alias), $"Validator alias '{v.Alias}' is not registered.");
                attribute = validators[v.Alias!](options)
                    ?? throw new JsonException($"Validator factory '{v.Alias}' returned null.");
            }
            else
            {
                PayloadJson.Require(v.Alias == null, "alias is only permitted for Custom validation.");
                attribute = PayloadJsonValidation.Create(v.Type!, options);
            }
            if (v.ErrorMessage != null) attribute.ErrorMessage = v.ErrorMessage;
            return attribute;
        }).ToArray() ?? [];

    private static void AddFields<T>(IFieldsBuilder<T> builder, IEnumerable<FieldDocument> fields)
    {
        foreach (var field in fields)
        {
            if (field.Type is { } type)
            {
                if (field.IsCollection)
                    builder.AddCollectionField(field.Name!, type, b =>
                    {
                        foreach (var attribute in field.Attributes) b.AddValidation(attribute);
                        foreach (var attribute in field.ItemAttributes) b.AddItemValidation(attribute);
                        if (field.EnumValues != null) b.AddEnumValues(field.EnumValues.ToArray());
                    });
                else
                    builder.AddField(field.Name!, type, b =>
                    {
                        foreach (var attribute in field.Attributes) b.AddValidation(attribute);
                        if (field.EnumValues != null) b.AddEnumValues(field.EnumValues.ToArray());
                    });
            }
            else if (field.IsCollection)
                builder.AddCollectionField(field.Name!, b =>
                {
                    foreach (var attribute in field.Attributes) b.AddValidation(attribute);
                    AddFields(b, field.Fields!);
                });
            else
                builder.AddObjectField(field.Name!, b =>
                {
                    foreach (var attribute in field.Attributes) b.AddValidation(attribute);
                    AddFields(b, field.Fields!);
                });
        }
    }

    private sealed class EventDocument
    {
        public string? FormatVersion { get; set; }
        public string? Name { get; set; }
        public List<FieldDocument>? Fields { get; set; }
        public ResponseDocument? Response { get; set; }
    }

    private sealed class ResponseDocument
    {
        public string? EventName { get; set; }
        public string? ClrType { get; set; }
    }

    private sealed class FieldDocument
    {
        public string? Name { get; set; }
        public FieldType? Type { get; set; }
        public bool IsCollection { get; set; }
        public List<FieldDocument>? Fields { get; set; }
        public List<string>? EnumValues { get; set; }
        public List<ValidatorDocument>? Validation { get; set; }
        public List<ValidatorDocument>? ItemValidation { get; set; }
        internal ValidationAttribute[] Attributes = [];
        internal ValidationAttribute[] ItemAttributes = [];
    }

    private sealed class ValidatorDocument
    {
        public string? Type { get; set; }
        public string? Alias { get; set; }
        public JsonElement Options { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
