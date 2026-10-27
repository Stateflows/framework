using System.Globalization;
using System.Text.Json;
using Stateflows.Common;

namespace Stateflows.Events.Json;

/// <summary>Creates structural events from strict JSON payloads using registered event definitions.</summary>
public static class JsonEventFactoryExtensions
{
    /// <summary>Creates a structural event from a JSON object payload and a registered event definition.</summary>
    /// <param name="factory">The event factory, which must also provide an event definition register.</param>
    /// <param name="name">The registered event name.</param>
    /// <param name="json">The JSON object containing field values.</param>
    /// <returns>The structural event created by the event factory.</returns>
    /// <exception cref="ArgumentException"><paramref name="factory"/> does not implement <see cref="IEventsRegister"/>.</exception>
    /// <exception cref="JsonException">The payload is malformed or a value does not conform to the registered field definition.</exception>
    /// <remarks>Values are parsed strictly without coercion. Unknown payload fields are ignored; duplicate JSON properties are rejected.</remarks>
    public static StructuralEvent CreateFromJson(this IEventFactory factory, string name, string json)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (factory is not IEventsRegister register)
            throw new ArgumentException("The factory must also implement IEventsRegister, or supply a register explicitly.", nameof(factory));
        return CreateFromJson(factory, name, json, register);
    }

    /// <summary>Creates a structural event when its definition register is a separate service.</summary>
    /// <param name="factory">The event factory that creates the structural event.</param>
    /// <param name="name">The registered event name.</param>
    /// <param name="json">The JSON object containing field values.</param>
    /// <param name="register">The register containing the event's field definition.</param>
    /// <returns>The structural event created by the event factory.</returns>
    /// <exception cref="JsonException">The payload is malformed or a value does not conform to the registered field definition.</exception>
    /// <remarks>Values are parsed strictly without coercion. Unknown payload fields are ignored; duplicate JSON properties are rejected.</remarks>
    public static StructuralEvent CreateFromJson(this IEventFactory factory, string name, string json, IEventsRegister register)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(register);
        var definition = register.GetEvent(name);
        using var document = EventJson.Parse(json);
        return factory.Create(name, ReadFields(document.RootElement, definition.Fields, "$"));
    }

    private static Dictionary<string, object> ReadFields(JsonElement value,
        IReadOnlyDictionary<string, FieldDefinition> fields, string path)
    {
        EventJson.Require(value.ValueKind == JsonValueKind.Object, $"'{path}' must be an object.");
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
            if (fields.TryGetValue(property.Name, out var field))
                result.Add(property.Name, ReadField(property.Value, field, $"{path}.{property.Name}")!);
        return result;
    }

    private static object? ReadField(JsonElement value, FieldDefinition field, string path)
    {
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (!field.IsCollection) return ReadItem(value, field, path);
        EventJson.Require(value.ValueKind == JsonValueKind.Array, $"'{path}' must be an array.");
        var index = 0;
        return value.EnumerateArray().Select(item => ReadItem(item, field, $"{path}[{index++}]")).ToArray();
    }

    private static object? ReadItem(JsonElement value, FieldDefinition field, string path)
    {
        if (value.ValueKind == JsonValueKind.Null) return null;
        if (field.Type == null) return ReadFields(value, field.Fields, path);
        try
        {
            switch (field.Type.Value)
            {
                case FieldType.Boolean:
                    EventJson.Require(value.ValueKind is JsonValueKind.True or JsonValueKind.False, $"'{path}' must be a boolean.");
                    return value.GetBoolean();
                case FieldType.Int32: Number(value, path); return value.GetInt32();
                case FieldType.Int64: Number(value, path); return value.GetInt64();
                case FieldType.Decimal: Number(value, path); return value.GetDecimal();
                case FieldType.Double:
                    Number(value, path);
                    var number = value.GetDouble();
                    EventJson.Require(double.IsFinite(number), $"'{path}' must be finite.");
                    return number;
            }
            EventJson.Require(value.ValueKind == JsonValueKind.String, $"'{path}' must be a string for {field.Type}.");
            var text = value.GetString()!;
            return field.Type.Value switch
            {
                FieldType.String => text,
                FieldType.Enum => field.EnumValues.Contains(text, StringComparer.Ordinal) ? text :
                    throw new JsonException($"'{path}' has an undeclared enum value."),
                FieldType.Guid => Guid.ParseExact(text, "D"),
                FieldType.DateTime => value.GetDateTime(),
                FieldType.DateTimeOffset => value.GetDateTimeOffset(),
                FieldType.TimeSpan => TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture),
                FieldType.DateOnly => DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                FieldType.TimeOnly => TimeOnly.ParseExact(text, ["HH:mm:ss", "HH:mm:ss.FFFFFFF"], CultureInfo.InvariantCulture),
                FieldType.Binary => DecodeBinary(text, path),
                _ => throw new JsonException($"Unsupported field type at '{path}'.")
            };
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or InvalidOperationException)
        {
            throw new JsonException($"Invalid {field.Type} value at '{path}'.", exception);
        }
    }

    private static byte[] DecodeBinary(string text, string path)
    {
        var bytes = Convert.FromBase64String(text);
        EventJson.Require(Convert.ToBase64String(bytes) == text, $"'{path}' must use canonical padded base64.");
        return bytes;
    }

    private static void Number(JsonElement value, string path)
        => EventJson.Require(value.ValueKind == JsonValueKind.Number, $"'{path}' must be a JSON number.");
}
