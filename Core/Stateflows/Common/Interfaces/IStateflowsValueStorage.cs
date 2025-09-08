using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsValueStorage
    {
        Task SetAsync<T>(BehaviorId behaviorId, string key, T value);

        Task<bool> IsSetAsync(BehaviorId behaviorId, string key);

        Task<bool> HasAnyPrefixedAsync(BehaviorId behaviorId, string prefix);

        Task<(bool Success, T Value)> TryGetAsync<T>(BehaviorId behaviorId, string key);

        Task<T> GetOrDefaultAsync<T>(BehaviorId behaviorId, string key, T defaultValue = default);

        Task UpdateAsync<T>(BehaviorId behaviorId, string key, Func<T, T> valueUpdater, T defaultValue = default);

        Task RemoveAsync(BehaviorId behaviorId, string key);

        Task RemovePrefixedAsync(BehaviorId behaviorId, string prefix);

        Task ClearAsync(BehaviorId behaviorId);
    }
}