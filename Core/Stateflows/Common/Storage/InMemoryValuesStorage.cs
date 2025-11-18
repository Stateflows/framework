using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Storage
{
    public class InMemoryValueStorage : IStateflowsValueStorage
    {
        private readonly Dictionary<string, Dictionary<BehaviorId, Dictionary<string, string>>> Values = new Dictionary<string, Dictionary<BehaviorId, Dictionary<string, string>>>();
        
        public InMemoryValueStorage(ITenantAccessor tenantAccessor)
        {
            TenantAccessor = tenantAccessor;
        }

        private readonly ITenantAccessor TenantAccessor;

        private Dictionary<string, string> GetBehaviorValues(BehaviorId behaviorId)
        {
            if (!Values.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantValues))
            {
                tenantValues = new Dictionary<BehaviorId, Dictionary<string, string>>();
                Values.Add(TenantAccessor.CurrentTenantId, tenantValues);
            }

            if (!tenantValues.TryGetValue(behaviorId, out var behaviorValues))
            {
                behaviorValues = new Dictionary<string, string>();
                tenantValues.Add(behaviorId, behaviorValues);
            }

            return behaviorValues;
        }
        
        public Task SetAsync<T>(BehaviorId behaviorId, string key, T value)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                InternalSet(key, value, behaviorValues);
            }
            
            return Task.CompletedTask;
        }

        private static void InternalSet<T>(string key, T value, Dictionary<string, string> behaviorValues)
        {
            behaviorValues[key] = StateflowsJsonConverter.SerializePolymorphicObject(value);
        }

        public Task<bool> IsSetAsync(BehaviorId behaviorId, string key)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                return Task.FromResult(behaviorValues.ContainsKey(key));
            }
        }

        public Task<bool> HasAnyPrefixedAsync(BehaviorId behaviorId, string prefix)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                return Task.FromResult(behaviorValues.Keys.Any(key => key.StartsWith(prefix)));
            }
        }

        public Task<(bool Success, T Value)> TryGetAsync<T>(BehaviorId behaviorId, string key)
        {
            (bool Success, T Value) result = (false, default);
            
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                if (!behaviorValues.TryGetValue(key, out var data))
                {
                    return Task.FromResult(result);
                }
                
                var type = typeof(T);
                var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                    ? ContextValuesCollection.ParseStringToTypedValue<T>(data)
                    : type.IsEnum
                        ? ContextValuesCollection.ParseStringToEnum<T>(data)
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

        public Task<T> GetOrDefaultAsync<T>(BehaviorId behaviorId, string key, T defaultValue = default)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                return Task.FromResult(InternalGetOrDefault(key, defaultValue, behaviorValues));
            }
        }

        private static T InternalGetOrDefault<T>(string key, T defaultValue, Dictionary<string, string> behaviorValues)
        {
            if (!behaviorValues.TryGetValue(key, out var data))
            {
                return defaultValue;
            }
            
            var type = typeof(T);
            var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                ? ContextValuesCollection.ParseStringToTypedValue<T>(data)
                : type.IsEnum
                    ? ContextValuesCollection.ParseStringToEnum<T>(data)
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

        public Task UpdateAsync<T>(BehaviorId behaviorId, string key, Func<T, T> valueUpdater, T defaultValue = default)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);
                
                var value = InternalGetOrDefault(key, defaultValue, behaviorValues);

                value = valueUpdater(value);

                InternalSet(key, value, behaviorValues);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(BehaviorId behaviorId, string key)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                behaviorValues.Remove(key);
            }
            
            return Task.CompletedTask;
        }

        public Task RemovePrefixedAsync(BehaviorId behaviorId, string prefix)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);
                
                var matchingKeys = behaviorValues.Keys.Where(key => key.StartsWith(prefix));
                foreach (var key in matchingKeys)
                {
                    behaviorValues.Remove(key);
                }
            }
            
            return Task.CompletedTask;
        }

        public Task ClearAsync(BehaviorId behaviorId)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);

                behaviorValues.Clear();
            }
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}