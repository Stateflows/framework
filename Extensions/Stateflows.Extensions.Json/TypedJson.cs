using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Stateflows.StateMachines.Json;

internal sealed class TypedJson(IReadOnlyDictionary<string, Type> types) : ISerializationBinder
{
    public Type Resolve(string alias)
        => types.TryGetValue(alias, out var type)
            ? type
            : throw new JsonSerializationException($"CLR alias '{alias}' is not registered.");

    public Type BindToType(string? assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName))
            throw new JsonSerializationException("Assembly-qualified type discriminators are not permitted.");
        return Resolve(typeName);
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        => throw new NotSupportedException("The definition loader does not serialize runtime objects.");

    private JsonSerializer Serializer() => JsonSerializer.Create(new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        SerializationBinder = this,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
        MissingMemberHandling = MissingMemberHandling.Error,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        DateParseHandling = DateParseHandling.None,
        MaxDepth = 64
    });

    private JToken Normalize(JToken value)
    {
        if (value is JArray array)
            return new JArray(array.Select(Normalize));
        if (value is not JObject obj) return value.DeepClone();
        foreach (var property in obj.Properties())
            if (property.Name is "$id" or "$ref" or "$values")
                throw new JsonSerializationException($"Serialization property '{property.Name}' is reserved.");
        var result = new JObject(obj.Properties().Select(p => new JProperty(p.Name, Normalize(p.Value))));
        if (result.TryGetValue("$type", out var discriminator))
        {
            var type = Resolve(discriminator.Value<string>()!);
            if (result.TryGetValue("$value", out var payload) &&
                type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                result.Remove("$value");
                result["$values"] = payload;
            }
        }
        return result;
    }

    public object? Read(JToken value, Type type)
    {
        var normalized = Normalize(value);
        if (normalized.Type == JTokenType.Null &&
            type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            throw new JsonSerializationException($"Null is not valid for '{type}'.");
        return normalized.ToObject(type, Serializer());
    }

    public object? ReadMetadata(JToken value)
    {
        if (value is JObject obj)
        {
            foreach (var property in obj.Properties())
                if (property.Name is "$id" or "$ref" or "$values")
                    throw new JsonSerializationException($"Serialization property '{property.Name}' is reserved.");
            if (obj.ContainsKey("$type")) return Read(obj, typeof(object));
            if (obj.ContainsKey("$value"))
                throw new JsonSerializationException("$value requires a $type discriminator.");
            return obj.Properties().ToDictionary(p => p.Name, p => ReadMetadata(p.Value), StringComparer.Ordinal);
        }
        if (value is JArray array) return array.Select(ReadMetadata).ToArray();
        return ((JValue)value).Value;
    }
}
