using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;

namespace Stateflows.Common.Classes
{
    internal class ContextValuesCollection : IContextValues
    {
        public Dictionary<string, string> Values { get; }

        public ContextValuesCollection(Dictionary<string, string> values)
        {
            Values = values;
        }

        private void InternalSet<T>(string key, T value)
        {
            Values[key] = StateflowsJsonConverter.SerializePolymorphicObject(value);
        }

        public void Set<T>(string key, T value)
        {
            lock (Values)
            {
                InternalSet(key, value);
            }
        }

        public bool IsSet(string key)
        {
            bool result;

            lock (Values)
            {
                result = Values.ContainsKey(key);
            }

            return result;
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            lock (Values)
            {
                if (!Values.TryGetValue(key, out var data))
                {
                    return false;
                }
                
                var type = typeof(T);
                var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                    ? ParseStringToTypedValue<T>(data)
                    : type.IsEnum
                        ? ParseStringToEnum<T>(data)
                        : StateflowsJsonConverter.DeserializeObject(data);

                if (type.IsNullable() && deserializedData is null)
                {
                    value = default;

                    return true;
                }
                    
                if (deserializedData is T t)
                {
                    value = t;

                    return true;
                }
            }

            return false;
        }

        private T InternalGetOrDefault<T>(string key, T defaultValue)
        {
            if (!Values.TryGetValue(key, out var data))
            {
                return defaultValue;
            }
            
            var type = typeof(T);
            var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                ? ParseStringToTypedValue<T>(data)
                : type.IsEnum
                    ? ParseStringToEnum<T>(data)
                    : StateflowsJsonConverter.DeserializeObject(data);

            if (type.IsNullable() && deserializedData is null)
            {
                return default;
            }

            if (deserializedData is T t)
            {
                return t;
            }

            return defaultValue;
        }

        public T GetOrDefault<T>(string key, T defaultValue = default)
        {
            lock (Values)
            {
                return InternalGetOrDefault(key, defaultValue);
            }
        }

        public void Update<T>(string key, Func<T, T> valueUpdater, T defaultValue = default)
        {
            lock (Values)
            {
                var value = InternalGetOrDefault(key, defaultValue);

                value = valueUpdater(value);

                InternalSet(key, value);
            }
        }

        public void Remove(string key)
        {
            lock (Values)
            {
                Values.Remove(key);
            }
        }

        public void Clear()
        {
            lock (Values)
            {
                Values.Clear();
            }
        }

        private static T ParseStringToEnum<T>(string value)
            => (T)(object)JToken.Parse(value).Value<int>();

        private static T ParseStringToTypedValue<T>(string value)
            => typeof(T) == typeof(string)
                ? JToken.Parse($"\"{value}\"").Value<T>()
                : JToken.Parse(value).Value<T>();
    }
}
