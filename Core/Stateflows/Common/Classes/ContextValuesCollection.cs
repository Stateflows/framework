using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public Task SetAsync<T>(string key, T value)
        {
            lock (Values)
            {
                InternalSet(key, value);
            }

            return Task.CompletedTask;
        }
        
        public Task<bool> IsSetAsync(string key)
        {
            bool result;

            lock (Values)
            {
                result = Values.ContainsKey(key);
            }

            return Task.FromResult(result);
        }
        
        public Task<(bool Success, T Value)> TryGetAsync<T>(string key)
        {
            (bool Success, T Value) result = (false, default);

            lock (Values)
            {
                if (!Values.TryGetValue(key, out var data))
                {
                    return Task.FromResult(result);
                }
                
                var type = typeof(T);
                var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                    ? ParseStringToTypedValue<T>(data)
                    : type.IsEnum
                        ? ParseStringToEnum<T>(data)
                        : type == typeof(Guid)
                            ? Guid.Parse(data)
                            : StateflowsJsonConverter.DeserializeObject(data);

                if (type.IsNullable() && deserializedData is null)
                {
                    result.Success = true;
                }
                else
                {
                    if (deserializedData is T t)
                    {
                        result.Value = t;
                        result.Success = true;
                    }
                }
            }

            return Task.FromResult(result);
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
                    : type == typeof(Guid)
                        ? Guid.Parse(data)
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

        public Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default)
        {
            T result;
            lock (Values)
            {
                result = InternalGetOrDefault(key, defaultValue);
            }
            
            return Task.FromResult(result);
        }

        public Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default)
        {
            lock (Values)
            {
                var value = InternalGetOrDefault(key, defaultValue);

                value = valueUpdater(value);

                InternalSet(key, value);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            lock (Values)
            {
                Values.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task RemovePrefixedAsync(string prefix)
        {
            lock (Values)
            {
                var matchingKeys = Values.Keys.Where(key => key.StartsWith(prefix));
                foreach (var key in matchingKeys)
                {
                    Values.Remove(key);
                }
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasAnyPrefixedAsync(string prefix)
        {
            lock (Values)
            {
                return Task.FromResult(Values.Keys.Any(key => key.StartsWith(prefix)));
            }
        }

        public Task ClearAsync()
        {
            lock (Values)
            {
                Values.Clear();
            }

            return Task.CompletedTask;
        }

        public static T ParseStringToEnum<T>(string value)
            => (T)(object)JToken.Parse(value).Value<int>();

        public static T ParseStringToTypedValue<T>(string value)
            => typeof(T) == typeof(string)
                ? JToken.Parse($"\"{value}\"").Value<T>()
                : JToken.Parse(value).Value<T>();
    }
}
