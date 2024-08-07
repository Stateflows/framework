﻿using Newtonsoft.Json.Linq;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using System.Collections.Generic;

namespace Stateflows.Common.Classes
{
    internal class ContextValuesCollection : IContextValues
    {
        public Dictionary<string, string> Values { get; }

        public ContextValuesCollection(Dictionary<string, string> values)
        {
            Values = values;
        }

        public void Set<T>(string key, T value)
        {
            lock (Values)
            {
                Values[key] = StateflowsJsonConverter.SerializePolymorphicObject(value);
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
                if (Values.TryGetValue(key, out var data))
                {
                    var type = typeof(T);
                    var deserializedData = type.IsPrimitive
                        ? ParseStringToTypedValue<T>(data)
                        : type.IsEnum
                            ? ParseStringToEnum<T>(data)
                            : StateflowsJsonConverter.DeserializeObject(data);

                    if (deserializedData is T t)
                    {
                        value = t;

                        return true;
                    }
                }
            }

            return false;
        }

        public T GetOrDefault<T>(string key, T defaultValue)
        {
            lock (Values)
            {
                if (Values.TryGetValue(key, out var data))
                {
                    var type = typeof(T);
                    var deserializedData = type.IsPrimitive
                        ? ParseStringToTypedValue<T>(data)
                        : type.IsEnum
                            ? ParseStringToEnum<T>(data)
                            : StateflowsJsonConverter.DeserializeObject(data);

                    if (deserializedData is T t)
                    {
                        return t;
                    }
                }
            }

            return defaultValue;
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
        {
            return (T)(object)JToken.Parse(value).Value<int>();
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
