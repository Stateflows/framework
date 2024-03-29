﻿using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class ContextValues : IContextValues
    {
        public Dictionary<string, string> Values { get; }

        public ContextValues(Dictionary<string, string> values)
        {
            Values = values;
        }

        public void Set<T>(string key, T value)
        {
            Values[key] = StateflowsJsonConverter.SerializePolymorphicObject(value);
        }

        public bool IsSet(string key)
        {
            return Values.ContainsKey(key);
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (Values.TryGetValue(key, out var data))
            {
                var type = typeof(T);
                var deserializedData = type.IsPrimitive
                    ? ParseStringToTypedValue<T>(data)
                    : StateflowsJsonConverter.DeserializeObject(data);

                if (deserializedData is T t)
                {
                    value = t;

                    return true;
                }
            }

            return false;
        }

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            if (Values.TryGetValue(key, out var data))
            {
                var type = typeof(T);
                var deserializedData = type.IsPrimitive
                    ? ParseStringToTypedValue<T>(data)
                    : StateflowsJsonConverter.DeserializeObject(data);

                if (deserializedData is T t)
                {
                    return t;
                }
            }

            return defaultValue;
        }

        public void Remove(string key)
        {
            Values.Remove(key);
        }

        public void Clear()
        {
            Values.Clear();
        }

        private static T ParseStringToTypedValue<T>(string value)
        {
            if (typeof(T) == typeof(string))
            {
                return JToken.Parse($"\"{value}\"").Value<T>();
            }

            return JToken.Parse(value).Value<T>();
        }
    }
}
