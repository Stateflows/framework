using System;
using Newtonsoft.Json.Linq;
using Stateflows.Common.Extensions;
using Stateflows.Common.Utilities;

namespace Stateflows.Common;

public class StateflowsValue
{
    public string Name { get; init; }
    public string Value { get; set; }
    public int Version { get; set; } = 0;
    public bool Changed { get; private set; } = false;
    public bool Removed { get; private set; } = false;

    public void Remove()
    {
        Removed = true;
    }

    public void Set<T>(T value)
    {
        var newValue = typeof(T) == typeof(Guid)
            ? ((Guid)(object)value).ToString()
            : StateflowsJsonConverter.SerializePolymorphicObject(value);

        if (Value != newValue)
        {
            Changed = true;
            Removed = false;
            Value = newValue;
        }
    }
        
    public bool TryGetAs<T>(out T result)
    {
        result = default;

        var type = typeof(T);
        var data = type.IsPrimitiveOrNullablePrimitive()
            ? ParseStringToTypedValue<T>(Value)
            : type.IsEnum
                ? ParseStringToEnum<T>(Value)
                : type == typeof(Guid)
                    ? Guid.Parse(Value)
                    : type == typeof(Guid)
                        ? Guid.Parse(Value)
                        : StateflowsJsonConverter.DeserializeObject(Value);
            
        if (type.IsNullable() && data is null)
        {
            return true;
        }
            
        if (data is T t)
        {
            result = t;
            return true;
        }

        return false;
    }

    public StateflowsValue Clone()
        => new()
        {
            Name = Name,
            Value = Value,
            Version = Version
        };

    private static T ParseStringToEnum<T>(string value)
        => (T)(object)JToken.Parse(value).Value<int>();

    private static T ParseStringToTypedValue<T>(string value)
        => typeof(T) == typeof(string)
            ? JToken.Parse($"\"{value}\"").Value<T>()
            : JToken.Parse(value).Value<T>();
}