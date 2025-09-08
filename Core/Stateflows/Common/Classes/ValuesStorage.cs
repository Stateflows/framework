using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    internal sealed class ValuesStorage : IContextValues
    {
        private readonly string Scope;

        private readonly BehaviorId BehaviorId;
        
        private readonly IStateflowsLock Lock;

        private readonly IStateflowsValueStorage Storage;
        
        private readonly TimeSpan LockTimeout = new TimeSpan(0, 0, 10);
        
        public ValuesStorage(string prefix, BehaviorId behaviorId, IStateflowsLock @lock, IStateflowsValueStorage storage)
        {
            Scope = prefix == string.Empty
                ? $"{nameof(ValuesStorage)}.Global"
                : $"{nameof(ValuesStorage)}.{prefix}";
            BehaviorId = behaviorId;
            Lock = @lock;
            Storage = storage;
        }

        private string GetKey(string key)
            => $"{Scope}.{key}";
        
        public async Task SetAsync<T>(string key, T value)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                await Storage.SetAsync(BehaviorId, GetKey(key), value);
            }
        }

        public async Task<bool> IsSetAsync(string key)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                return await Storage.IsSetAsync(BehaviorId, GetKey(key));
            }
        }

        public async Task<bool> HasAnyPrefixedAsync(string prefix)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                return await Storage.HasAnyPrefixedAsync(BehaviorId, prefix);
            }
        }

        public async Task<(bool Success, T Value)> TryGetAsync<T>(string key)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                return await Storage.TryGetAsync<T>(BehaviorId, GetKey(key));
            }
        }

        public async Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                return await Storage.GetOrDefaultAsync<T>(BehaviorId, GetKey(key));
            }
        }

        public async Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                await Storage.UpdateAsync<T>(BehaviorId, GetKey(key), valueUpdater, defaultValue);
            }
        }

        public async Task RemoveAsync(string key)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                await Storage.RemoveAsync(BehaviorId, GetKey(key));
            }
        }

        public async Task RemovePrefixedAsync(string prefix)
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                await Storage.RemovePrefixedAsync(BehaviorId, GetKey(prefix));
            }
        }

        public async Task ClearAsync()
        {
            await using (await Lock.AquireLockAsync(BehaviorId, Scope, LockTimeout))
            {
                await Storage.RemovePrefixedAsync(BehaviorId, $"{Scope}.");
            }
        }
    }
}