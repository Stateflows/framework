using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stateflows.Common
{
    // Internal persistence preserves scalar widths and formats; public JSON payloads use the JSON adapter.
    internal sealed class StructuralFieldsConverter : JsonConverter<Dictionary<string, object>>
    {
        public override void WriteJson(JsonWriter writer, Dictionary<string, object> value, JsonSerializer serializer)
            => WriteFields(value).WriteTo(writer);

        private static JObject WriteFields(IReadOnlyDictionary<string, object> fields)
            => new(fields.Select(field => new JProperty(field.Key, WriteValue(field.Value))));

        private static JToken WriteValue(object value)
        {
            if (value == null) return JValue.CreateNull();
            if (value is StructuralValue structure)
                return new JObject { ["kind"] = "Object", ["value"] = WriteFields(structure.Fields) };
            if (value is IEnumerable collection && value is not string && value is not byte[])
                return new JObject { ["kind"] = "Collection", ["value"] = new JArray(collection.Cast<object>().Select(WriteValue)) };
            var (kind, token) = value switch
            {
                string text => ("String", new JValue(text)),
                bool flag => ("Boolean", new JValue(flag)),
                int number => ("Int32", new JValue(number)),
                long number => ("Int64", new JValue(number)),
                decimal number => ("Decimal", new JValue(number.ToString(CultureInfo.InvariantCulture))),
                double number => ("Double", new JValue(number.ToString("R", CultureInfo.InvariantCulture))),
                Guid guid => ("Guid", new JValue(guid.ToString("D"))),
                DateTime date => ("DateTime", new JValue(date.ToString("O", CultureInfo.InvariantCulture))),
                DateTimeOffset date => ("DateTimeOffset", new JValue(date.ToString("O", CultureInfo.InvariantCulture))),
                TimeSpan time => ("TimeSpan", new JValue(time.ToString("c", CultureInfo.InvariantCulture))),
                DateOnly date => ("DateOnly", new JValue(date.ToString("O", CultureInfo.InvariantCulture))),
                TimeOnly time => ("TimeOnly", new JValue(time.ToString("O", CultureInfo.InvariantCulture))),
                byte[] bytes => ("Binary", new JValue(Convert.ToBase64String(bytes))),
                _ => throw new JsonSerializationException($"Unsupported structural scalar '{value.GetType()}'.")
            };
            return new JObject { ["kind"] = kind, ["value"] = token };
        }

        public override Dictionary<string, object> ReadJson(JsonReader reader, Type objectType,
            Dictionary<string, object> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var dateHandling = reader.DateParseHandling;
            reader.DateParseHandling = DateParseHandling.None;
            try { return ReadFields(JObject.Load(reader)); }
            finally { reader.DateParseHandling = dateHandling; }
        }

        private static Dictionary<string, object> ReadFields(JObject fields)
            => fields.Properties().ToDictionary(field => field.Name, field => ReadValue(field.Value), StringComparer.Ordinal);

        private static object ReadValue(JToken node)
        {
            if (node.Type == JTokenType.Null) return null;
            var value = node["value"];
            var text = value is JValue scalar ? scalar.Value<string>() : null;
            return node["kind"]?.Value<string>() switch
            {
                "Object" => new StructuralValue(ReadFields((JObject)value)),
                "Collection" => ((JArray)value).Select(ReadValue).ToArray(),
                "String" => text,
                "Boolean" => value.Value<bool>(),
                "Int32" => value.Value<int>(),
                "Int64" => value.Value<long>(),
                "Decimal" => decimal.Parse(text, CultureInfo.InvariantCulture),
                "Double" => double.Parse(text, CultureInfo.InvariantCulture),
                "Guid" => Guid.ParseExact(text, "D"),
                "DateTime" => DateTime.ParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                "DateTimeOffset" => DateTimeOffset.ParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.None),
                "TimeSpan" => TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture),
                "DateOnly" => DateOnly.ParseExact(text, "O", CultureInfo.InvariantCulture),
                "TimeOnly" => TimeOnly.ParseExact(text, "O", CultureInfo.InvariantCulture),
                "Binary" => Convert.FromBase64String(text),
                _ => throw new JsonSerializationException("Unknown structural persistence value kind.")
            };
        }
    }
}
