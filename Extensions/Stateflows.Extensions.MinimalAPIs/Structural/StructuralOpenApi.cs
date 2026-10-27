using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>
/// Builds native OpenAPI descriptions of structural event endpoints from <see cref="EventDefinition"/>s.
/// </summary>
internal sealed class StructuralOpenApi(IReadOnlyDictionary<Type, Action<ValidationAttribute, OpenApiSchema>> validationSchemaMappers)
{

    private static readonly (int StatusCode, string Description)[] EnvelopeResponses =
    [
        (200, "Event consumed (Consumed, Duplicated)"),
        (201, "Behavior initialized (Initialized)"),
        (202, "Event accepted for later processing (Deferred, Forwarded)"),
        (400, "Event is invalid (Invalid)"),
        (404, "Behavior not found (Undelivered)"),
        (409, "Event not accepted in current state (NotInitialized, Rejected, NotConsumed)"),
        (500, "Event processing failed (Failed)"),
        (503, "Behavior overloaded, retry later (Overloaded)"),
    ];

    internal static string DescribeType(FieldType fieldType)
        => fieldType switch
        {
            FieldType.String => "a string",
            FieldType.Enum => "a string",
            FieldType.Boolean => "a boolean",
            FieldType.Int32 => "a 32-bit integer",
            FieldType.Int64 => "a 64-bit integer",
            FieldType.Decimal => "a decimal number",
            FieldType.Double => "a finite number",
            FieldType.Guid => "a GUID string",
            FieldType.DateTime => "an ISO 8601 date-time string",
            FieldType.DateTimeOffset => "an ISO 8601 date-time string",
            FieldType.TimeSpan => "a time span string (e.g. '1.02:03:04')",
            FieldType.DateOnly => "a date string (yyyy-MM-dd)",
            FieldType.TimeOnly => "a time string (HH:mm:ss)",
            FieldType.Binary => "a base64 string",
            _ => fieldType.ToString()
        };

    public async Task ApplyAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, EventDefinition definition, CancellationToken cancellationToken)
    {
        operation.Summary ??= $"Sends '{definition.Name}' {(definition.IsRequest ? "request" : "event")}";

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = definition.Fields.Values.Any(IsRequired),
            Description = $"Envelope containing '{definition.Name}' event fields and optional list of requested notifications",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new() { Schema = CreateRequestEnvelopeSchema(definition) }
            }
        };

        IOpenApiSchema? responseSchema = null;
        if (definition.ResponseType != null)
        {
            responseSchema = await context.GetOrCreateSchemaAsync(definition.ResponseType, null, cancellationToken);
        }
        else if (definition.ResponseEventName != null)
        {
            var register = context.ApplicationServices.GetService<IEventsRegister>();
            if (register != null && register.Events.TryGetValue(definition.ResponseEventName, out var responseDefinition))
            {
                responseSchema = CreateEventSchema(responseDefinition);
            }
        }

        operation.Responses ??= new OpenApiResponses();
        foreach (var (statusCode, description) in EnvelopeResponses)
        {
            var response = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = CreateResponseEnvelopeSchema(
                            statusCode == 200 ? responseSchema : null,
                            includeValidation: statusCode == 400
                        )
                    }
                }
            };

            if (statusCode == 200)
            {
                response.Content["text/event-stream"] = new()
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Description = "Server-sent events stream of requested notifications (when 'stream=true')"
                    }
                };
            }

            operation.Responses[statusCode.ToString(CultureInfo.InvariantCulture)] = response;
        }
    }

    internal OpenApiSchema CreateRequestEnvelopeSchema(EventDefinition definition)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["event"] = CreateEventSchema(definition),
                ["requestedNotifications"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array | JsonSchemaType.Null,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String },
                    Description = "Names of notifications to be collected while processing the event"
                }
            }
        };

        if (definition.Fields.Values.Any(IsRequired))
        {
            schema.Required = new HashSet<string> { "event" };
        }

        return schema;
    }

    internal OpenApiSchema CreateEventSchema(EventDefinition definition)
    {
        var schema = CreateObjectSchema(definition.Fields);
        schema.Title = definition.Name;
        return schema;
    }

    private OpenApiSchema CreateObjectSchema(IReadOnlyDictionary<string, FieldDefinition> fields)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
        };

        foreach (var field in fields.Values)
        {
            schema.Properties[field.Name] = CreateFieldSchema(field);
            if (IsRequired(field))
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(field.Name);
            }
        }

        return schema;
    }

    private static bool IsRequired(FieldDefinition field)
        => field.Validation.OfType<RequiredAttribute>().Any();

    private enum ValueKind
    {
        Text,
        OtherString,
        Binary,
        Integer,
        Number,
        Boolean,
        Object,
        Array
    }

    private static ValueKind GetItemKind(FieldDefinition field)
        => field.Type switch
        {
            null => ValueKind.Object,
            FieldType.String or FieldType.Enum => ValueKind.Text,
            FieldType.Binary => ValueKind.Binary,
            FieldType.Int32 or FieldType.Int64 => ValueKind.Integer,
            FieldType.Decimal or FieldType.Double => ValueKind.Number,
            FieldType.Boolean => ValueKind.Boolean,
            _ => ValueKind.OtherString
        };

    private static Type? GetClrType(FieldDefinition field)
        => field.Type switch
        {
            FieldType.String or FieldType.Enum => typeof(string),
            FieldType.Boolean => typeof(bool),
            FieldType.Int32 => typeof(int),
            FieldType.Int64 => typeof(long),
            FieldType.Decimal => typeof(decimal),
            FieldType.Double => typeof(double),
            FieldType.Guid => typeof(Guid),
            FieldType.DateTime => typeof(DateTime),
            FieldType.DateTimeOffset => typeof(DateTimeOffset),
            FieldType.TimeSpan => typeof(TimeSpan),
            FieldType.DateOnly => typeof(DateOnly),
            FieldType.TimeOnly => typeof(TimeOnly),
            _ => null
        };

    private OpenApiSchema CreateFieldSchema(FieldDefinition field)
    {
        if (!field.IsCollection)
        {
            var scalar = CreateItemSchema(field);
            if (!IsRequired(field))
            {
                MakeNullable(scalar);
            }

            ApplyValidation(scalar, field.Validation, GetItemKind(field), field);
            return scalar;
        }

        var itemSchema = CreateItemSchema(field);
        if (!field.ItemValidation.OfType<RequiredAttribute>().Any())
        {
            MakeNullable(itemSchema);
        }

        ApplyValidation(itemSchema, field.ItemValidation, GetItemKind(field), field);

        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = itemSchema
        };
        if (!IsRequired(field))
        {
            MakeNullable(schema);
        }

        ApplyValidation(schema, field.Validation, ValueKind.Array, field);

        return schema;
    }

    private static void MakeNullable(OpenApiSchema schema)
    {
        schema.Type |= JsonSchemaType.Null;
        if (schema.Enum is { Count: > 0 })
        {
            schema.Enum.Add(null!);
        }
    }

    private OpenApiSchema CreateItemSchema(FieldDefinition field)
    {
        if (field.Type == null)
        {
            return CreateObjectSchema(field.Fields);
        }

        // Shapes follow native ASP.NET Core OpenAPI (.NET 10) schemas of corresponding CLR types
        var schema = field.Type.Value switch
        {
            FieldType.String => new OpenApiSchema { Type = JsonSchemaType.String },
            FieldType.Enum => new OpenApiSchema { Type = JsonSchemaType.String },
            FieldType.Boolean => new OpenApiSchema { Type = JsonSchemaType.Boolean },
            FieldType.Int32 => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
            FieldType.Int64 => new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int64" },
            FieldType.Decimal => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
            FieldType.Double => new OpenApiSchema { Type = JsonSchemaType.Number, Format = "double" },
            FieldType.Guid => new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
            FieldType.DateTime => new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
            FieldType.DateTimeOffset => new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" },
            FieldType.TimeSpan => new OpenApiSchema { Type = JsonSchemaType.String, Pattern = @"^-?(\d+\.)?\d{2}:\d{2}:\d{2}(\.\d{1,7})?$" },
            FieldType.DateOnly => new OpenApiSchema { Type = JsonSchemaType.String, Format = "date" },
            FieldType.TimeOnly => new OpenApiSchema { Type = JsonSchemaType.String, Format = "time" },
            FieldType.Binary => new OpenApiSchema { Type = JsonSchemaType.String, Format = "byte" },
            _ => new OpenApiSchema()
        };

        if (field.Type == FieldType.Enum)
        {
            schema.Enum = field.EnumValues.Select(v => (JsonNode)JsonValue.Create(v)).ToList();
        }

        return schema;
    }

    private void ApplyValidation(OpenApiSchema schema, IEnumerable<ValidationAttribute> attributes, ValueKind kind, FieldDefinition field)
    {
        var unmapped = new List<string>();
        foreach (var attribute in attributes)
        {
            if (TryApplyCustomMapping(schema, attribute, field))
            {
                continue;
            }

            if (!TryApplyBuiltInMapping(schema, attribute, kind, field))
            {
                var name = attribute.GetType().Name;
                if (name.EndsWith("Attribute", StringComparison.Ordinal) && name.Length > "Attribute".Length)
                {
                    name = name[..^"Attribute".Length];
                }

                if (!unmapped.Contains(name))
                {
                    unmapped.Add(name);
                }
            }
        }

        if (unmapped.Count > 0)
        {
            var note = $"Additional validation not described by this schema: {string.Join(", ", unmapped)}.";
            schema.Description = string.IsNullOrEmpty(schema.Description) ? note : $"{schema.Description} {note}";
        }
    }

    private bool TryApplyCustomMapping(OpenApiSchema schema, ValidationAttribute attribute, FieldDefinition field)
    {
        for (var type = attribute.GetType(); type != null && type != typeof(ValidationAttribute); type = type.BaseType)
        {
            if (validationSchemaMappers.TryGetValue(type, out var mapper))
            {
                try
                {
                    mapper(attribute, schema);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Validation schema mapper registered for '{type.FullName}' failed for attribute '{attribute.GetType().FullName}' on structural field '{field.Name}'.",
                        exception);
                }

                return true;
            }
        }

        return false;
    }

    private static bool TryApplyBuiltInMapping(OpenApiSchema schema, ValidationAttribute attribute, ValueKind kind, FieldDefinition field)
    {
        // Derived attributes may override IsValid, so only the framework types themselves are described automatically
        if (attribute.GetType().Assembly != typeof(ValidationAttribute).Assembly)
        {
            return false;
        }

        switch (attribute)
        {
            case RequiredAttribute required:
                // Requiredness is expressed by 'required' list of containing object (or non-nullable items)
                if (!required.AllowEmptyStrings && kind == ValueKind.Text)
                {
                    schema.MinLength = Math.Max(schema.MinLength ?? 0, 1);
                    AddPattern(schema, @"\S");
                }
                return true;

            case StringLengthAttribute stringLength when kind == ValueKind.Text:
                SetLength(schema, stringLength.MinimumLength > 0 ? stringLength.MinimumLength : null, stringLength.MaximumLength, isArray: false);
                return true;

            case MinLengthAttribute minLength when kind is ValueKind.Text or ValueKind.Array:
                SetLength(schema, minLength.Length, null, kind == ValueKind.Array);
                return true;

            case MaxLengthAttribute maxLength when kind is ValueKind.Text or ValueKind.Array:
                SetLength(schema, null, maxLength.Length > 0 ? maxLength.Length : null, kind == ValueKind.Array);
                return true;

            case LengthAttribute length when kind is ValueKind.Text or ValueKind.Array:
                SetLength(schema, length.MinimumLength, length.MaximumLength, kind == ValueKind.Array);
                return true;

            case RangeAttribute range when kind is ValueKind.Integer or ValueKind.Number:
                return TryApplyRange(schema, range);

            case RegularExpressionAttribute regex when kind == ValueKind.Text:
                // RegularExpressionAttribute requires the whole value to match
                AddPattern(schema, $"^(?:{regex.Pattern})$");
                return true;

            case EmailAddressAttribute when kind == ValueKind.Text:
                // Mirrors EmailAddressAttribute: single '@', neither first nor last, no line breaks
                AddPattern(schema, @"^[^@\r\n]+@[^@\r\n]+$");
                return true;

            case UrlAttribute when kind == ValueKind.Text:
                // Mirrors UrlAttribute: case-insensitive http://, https:// or ftp:// prefix
                AddPattern(schema, "^([hH][tT][tT][pP][sS]?|[fF][tT][pP])://");
                return true;

            case Base64StringAttribute when kind is ValueKind.Text or ValueKind.Binary:
                schema.Format = "byte";
                return true;

            case AllowedValuesAttribute allowed when TryGetValueNodes(allowed.Values, field, kind, out var allowedNodes):
                schema.Enum = schema.Enum is { Count: > 0 } existing
                    ? existing.Where(e => e != null && allowedNodes.Any(a => JsonNode.DeepEquals(a, e))).ToList()
                    : allowedNodes;
                return true;

            case DeniedValuesAttribute denied when TryGetValueNodes(denied.Values, field, kind, out var deniedNodes):
                AddAllOf(schema, new OpenApiSchema { Not = new OpenApiSchema { Enum = deniedNodes } });
                return true;

            default:
                return false;
        }
    }

    private static bool TryGetValueNodes(object?[] values, FieldDefinition field, ValueKind kind, out List<JsonNode> nodes)
    {
        nodes = [];
        var clrType = GetClrType(field);
        if (kind is ValueKind.Array or ValueKind.Object or ValueKind.Binary || clrType == null)
        {
            return false;
        }

        foreach (var value in values)
        {
            // Allowed/denied values are compared with Equals, so only values of exact field type are representable
            if (value == null || value.GetType() != clrType)
            {
                return false;
            }

            nodes.Add(JsonSerializer.SerializeToNode(value, clrType)!);
        }

        return true;
    }

    private static void AddPattern(OpenApiSchema schema, string pattern)
    {
        if (schema.Pattern == null)
        {
            schema.Pattern = pattern;
        }
        else if (schema.Pattern != pattern)
        {
            AddAllOf(schema, new OpenApiSchema { Pattern = pattern });
        }
    }

    private static void AddAllOf(OpenApiSchema schema, OpenApiSchema constraint)
    {
        schema.AllOf ??= new List<IOpenApiSchema>();
        schema.AllOf.Add(constraint);
    }

    private static void SetLength(OpenApiSchema schema, int? min, int? max, bool isArray)
    {
        if (isArray)
        {
            if (min != null) schema.MinItems = Math.Max(schema.MinItems ?? 0, min.Value);
            if (max != null) schema.MaxItems = schema.MaxItems == null ? max : Math.Min(schema.MaxItems.Value, max.Value);
        }
        else
        {
            if (min != null) schema.MinLength = Math.Max(schema.MinLength ?? 0, min.Value);
            if (max != null) schema.MaxLength = schema.MaxLength == null ? max : Math.Min(schema.MaxLength.Value, max.Value);
        }
    }

    private static bool TryApplyRange(OpenApiSchema schema, RangeAttribute range)
    {
        var culture = range.ParseLimitsInInvariantCulture ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
        var numericOperand = range.OperandType == typeof(int) || range.OperandType == typeof(double) ||
            range.OperandType == typeof(long) || range.OperandType == typeof(decimal) ||
            range.OperandType == typeof(float) || range.OperandType == typeof(short) || range.OperandType == typeof(byte);
        if (!numericOperand)
        {
            return false;
        }

        var minimum = ToInvariantNumber(range.Minimum, culture);
        var maximum = ToInvariantNumber(range.Maximum, culture);
        if (minimum == null || maximum == null)
        {
            return false;
        }

        if (range.MinimumIsExclusive)
        {
            schema.ExclusiveMinimum = minimum;
        }
        else
        {
            schema.Minimum = minimum;
        }

        if (range.MaximumIsExclusive)
        {
            schema.ExclusiveMaximum = maximum;
        }
        else
        {
            schema.Maximum = maximum;
        }

        return true;
    }

    private static string? ToInvariantNumber(object? value, CultureInfo culture)
        => value switch
        {
            int or long or short or byte or decimal => Convert.ToString(value, CultureInfo.InvariantCulture),
            double d when double.IsFinite(d) => d.ToString("R", CultureInfo.InvariantCulture),
            float f when float.IsFinite(f) => f.ToString("R", CultureInfo.InvariantCulture),
            string s when decimal.TryParse(s, NumberStyles.Float, culture, out var parsed) => parsed.ToString(CultureInfo.InvariantCulture),
            _ => null
        };
    private static OpenApiSchema CreateResponseEnvelopeSchema(IOpenApiSchema? responseSchema, bool includeValidation)
    {
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["status"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Integer,
                    Format = "int32",
                    Description = "Numeric value of Stateflows EventStatus"
                },
                ["statusText"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum = Enum.GetNames<EventStatus>().Select(n => (JsonNode)JsonValue.Create(n)).ToList()
                },
                ["notifications"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.Object },
                    Description = "Notifications requested in 'requestedNotifications', published while processing the event"
                },
                ["links"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["rel"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["href"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["method"] = new OpenApiSchema { Type = JsonSchemaType.String },
                        }
                    },
                    Description = "HATEOAS links available in current state of the behavior"
                },
                ["metadata"] = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                    AdditionalProperties = new OpenApiSchema()
                }
            },
            Required = new HashSet<string> { "status", "statusText" }
        };

        if (includeValidation)
        {
            schema.Properties["eventValidation"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["isValid"] = new OpenApiSchema { Type = JsonSchemaType.Boolean },
                    ["validationResults"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Array,
                        Items = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Object,
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                ["errorMessage"] = new OpenApiSchema { Type = JsonSchemaType.String },
                                ["memberNames"] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Array,
                                    Items = new OpenApiSchema { Type = JsonSchemaType.String }
                                },
                            }
                        }
                    }
                }
            };
        }

        if (responseSchema != null)
        {
            schema.Properties["response"] = responseSchema;
        }

        return schema;
    }
}
