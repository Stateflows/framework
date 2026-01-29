using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Storage
{
    public class InMemoryValueStorage : IStateflowsValueStorage
    {
        private readonly Dictionary<string, Dictionary<BehaviorId, Dictionary<string, StateflowsValue>>> Values = new();
        
        public InMemoryValueStorage(ITenantAccessor tenantAccessor)
        {
            TenantAccessor = tenantAccessor;
        }

        private readonly ITenantAccessor TenantAccessor;

        private Dictionary<string, StateflowsValue> GetBehaviorValues(BehaviorId behaviorId)
        {
            if (!Values.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantValues))
            {
                tenantValues = new Dictionary<BehaviorId, Dictionary<string, StateflowsValue>>();
                Values.Add(TenantAccessor.CurrentTenantId, tenantValues);
            }

            if (!tenantValues.TryGetValue(behaviorId, out var behaviorValues))
            {
                behaviorValues = new Dictionary<string, StateflowsValue>();
                tenantValues.Add(behaviorId, behaviorValues);
            }

            return behaviorValues;
        }

        public Task<IReadOnlyDictionary<string, StateflowsValue>> LoadAsync(BehaviorId behaviorId)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);
                return Task.FromResult<IReadOnlyDictionary<string, StateflowsValue>>(
                    behaviorValues
                        .ToDictionary(
                            p => p.Key,
                            p => p.Value.Clone()
                        )
                        .AsReadOnly()
                );
            }
        }

        public async Task SaveAsync(BehaviorId behaviorId, IReadOnlyDictionary<string, StateflowsValue> values)
        {
            lock (Values)
            {
                var behaviorValues = GetBehaviorValues(behaviorId);
                foreach (var value in values.Values.Where(v => v.Removed))
                {
                    if (behaviorValues.TryGetValue(value.Name, out var stateflowsValue))
                    {
                        if (stateflowsValue.Version != value.Version)
                        {
                            throw new StateflowsRuntimeException("Value version mismatch.");
                        }

                        behaviorValues.Remove(value.Name);
                    }
                }

                foreach (var value in values.Values.Where(v => v.Changed && !v.Removed))
                {
                    if (!behaviorValues.TryGetValue(value.Name, out var stateflowsValue))
                    {
                        stateflowsValue = new StateflowsValue()
                        {
                            Name = value.Name
                        };
                    }

                    if (stateflowsValue.Version != value.Version)
                    {
                        throw new StateflowsRuntimeException("Value version mismatch.");
                    }
                    
                    stateflowsValue.Value = value.Value;
                    stateflowsValue.Version++;
                    behaviorValues[value.Name] = stateflowsValue;
                }
            }
        }

        public async Task<IReadOnlyDictionary<string, StateflowsValue>> SaveAndLoadAsync(BehaviorId behaviorId, IReadOnlyDictionary<string, StateflowsValue> values)
        {
            await SaveAsync(behaviorId, values);
            return await LoadAsync(behaviorId);
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

        private static void InternalSet<T>(string key, T value, Dictionary<string, StateflowsValue> behaviorValues)
        {
            if (!behaviorValues.TryGetValue(key, out var stateflowsValue))
            {
                stateflowsValue = new StateflowsValue()
                {
                    Name = key,
                    Version = 1
                };
                
                behaviorValues.Add(key, stateflowsValue);
            }
            
            stateflowsValue.Set(value);
            // stateflowsValue.Value = typeof(T) == typeof(Guid)
            //     ? ((Guid)(object)value).ToString()
            //     : StateflowsJsonConverter.SerializePolymorphicObject(value);
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

                result.Success = data.TryGetAs(out result.Value);
                // var type = typeof(T);
                // var deserializedData = type.IsPrimitiveOrNullablePrimitive()
                //     ? ContextValuesCollection.ParseStringToTypedValue<T>(data.Value)
                //     : type.IsEnum
                //         ? ContextValuesCollection.ParseStringToEnum<T>(data.Value)
                //         : type == typeof(Guid)
                //             ? Guid.Parse(data.Value)
                //             : type == typeof(Guid)
                //                 ? Guid.Parse(data.Value)
                //                 : StateflowsJsonConverter.DeserializeObject(data.Value);
                //
                // if (type.IsNullable() && deserializedData is null)
                // {
                //     result.Success = true;
                // }
                // else
                // {
                //     if (deserializedData is T t)
                //     {
                //         result.Value = t;
                //         result.Success = true;
                //     }
                // }
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

        private static T InternalGetOrDefault<T>(string key, T defaultValue, Dictionary<string, StateflowsValue> behaviorValues)
        {
            if (!behaviorValues.TryGetValue(key, out var data))
            {
                return defaultValue;
            }
            
            return data.TryGetAs(out T result)
                ? result
                : defaultValue;
            
            // var type = typeof(T);
            // var deserializedData = type.IsPrimitiveOrNullablePrimitive()
            //     ? ContextValuesCollection.ParseStringToTypedValue<T>(data)
            //     : type.IsEnum
            //         ? ContextValuesCollection.ParseStringToEnum<T>(data)
            //         : type == typeof(Guid)
            //             ? Guid.Parse(data)
            //             : StateflowsJsonConverter.DeserializeObject(data);
            //
            // if (type.IsNullable() && deserializedData is null)
            // {
            //     return default;
            // }
            //
            // if (deserializedData is T t)
            // {
            //     return t;
            // }

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