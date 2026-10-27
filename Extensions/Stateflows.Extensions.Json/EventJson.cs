using System.Text.Json;
using System.Text.Json.Serialization;
using Stateflows.Common;

namespace Stateflows.Events.Json;

internal static class EventJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new FieldTypeConverter() },
        MaxDepth = 64
    };

    internal static JsonDocument Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64 });
        try { CheckDuplicates(document.RootElement, "$"); }
        catch { document.Dispose(); throw; }
        return document;
    }

    private static void CheckDuplicates(JsonElement value, string path)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in value.EnumerateObject())
            {
                var child = $"{path}.{property.Name}";
                if (!names.Add(property.Name)) throw new JsonException($"Duplicate property at '{child}'.");
                CheckDuplicates(property.Value, child);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in value.EnumerateArray()) CheckDuplicates(item, $"{path}[{index++}]");
        }
    }

    internal static T Read<T>(JsonElement element) where T : class
        => element.Deserialize<T>(Options) ?? throw new JsonException($"Expected {typeof(T).Name} object.");

    internal static void Require(bool condition, string message)
    {
        if (!condition) throw new JsonException(message);
    }

    private sealed class FieldTypeConverter : JsonConverter<FieldType>
    {
        public override FieldType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String &&
                Enum.TryParse<FieldType>(reader.GetString(), false, out var value) &&
                Enum.IsDefined(value) && reader.GetString() == value.ToString())
                return value;
            throw new JsonException("Field type must be an exact, case-sensitive FieldType name.");
        }

        public override void Write(Utf8JsonWriter writer, FieldType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}
