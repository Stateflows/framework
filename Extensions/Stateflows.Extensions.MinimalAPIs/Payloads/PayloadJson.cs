using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>
/// Serializes payload values (events, nested objects) as plain JSON objects containing only their fields,
/// preserving declared field names.
/// </summary>
internal sealed class PayloadValueJsonConverter : JsonConverter
{
    public override bool CanRead => false;

    public override bool CanConvert(Type objectType)
        => typeof(PayloadValue).IsAssignableFrom(objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not PayloadValue payloadValue)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartObject();
        foreach (var (name, fieldValue) in payloadValue.Fields)
        {
            writer.WritePropertyName(name);
            WriteValue(writer, fieldValue, serializer);
        }
        writer.WriteEndObject();
    }

    private static void WriteValue(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        switch (value)
        {
            case null:
                writer.WriteNull();
                break;
            case DateOnly dateOnly:
                writer.WriteValue(dateOnly.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case TimeOnly timeOnly:
                writer.WriteValue(timeOnly.ToString("HH:mm:ss.FFFFFFF", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case TimeSpan timeSpan:
                writer.WriteValue(timeSpan.ToString("c", System.Globalization.CultureInfo.InvariantCulture));
                break;
            case byte[] bytes:
                writer.WriteValue(Convert.ToBase64String(bytes));
                break;
            case string or PayloadValue:
                serializer.Serialize(writer, value);
                break;
            case System.Collections.IEnumerable items:
                writer.WriteStartArray();
                foreach (var item in items)
                {
                    WriteValue(writer, item, serializer);
                }
                writer.WriteEndArray();
                break;
            default:
                serializer.Serialize(writer, value);
                break;
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        => throw new NotSupportedException("Payload values cannot be deserialized directly; use IPayloadFactory.");
}

internal static class PayloadJson
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = [new PayloadValueJsonConverter()]
    };

    /// <summary>
    /// Serializes Minimal APIs envelopes in the same manner as <see cref="Stateflows.Common.Utilities.StateflowsJsonConverter.SerializeObject"/>
    /// with camelCase names, but renders payload values as plain field objects.
    /// </summary>
    public static string SerializeObject(object value)
        => JsonConvert.SerializeObject(value, null, Settings);
}
