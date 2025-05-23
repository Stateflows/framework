using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface INamespace
    {
        [Obsolete("SetAsync() is obsolete, use GetValue(key).SetAsync() instead")]
        Task SetAsync<T>(string key, T value);

        [Obsolete("IsSetAsync() is obsolete, use GetValue(key).IsSetAsync() instead")]
        Task<bool> IsSetAsync(string key);

        [Obsolete("TryGetAsync() is obsolete, use GetValue(key).TryGetAsync() instead")]
        Task<(bool Success, T Value)> TryGetAsync<T>(string key);

        [Obsolete("TryGetAsync() is obsolete, use GetValue(key).TryGetAsync() instead")]
        Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default);

        [Obsolete("UpdateAsync() is obsolete, use GetValue(key).UpdateAsync() instead")]
        Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default);

        [Obsolete("RemoveAsync() is obsolete, use GetValue(key).RemoveAsync() instead")]
        Task RemoveAsync(string key);
        
        IValue<T> GetValue<T>(string key);
        
        INamespace GetNamespace(string namespaceName);

        Task ClearAsync();
    }
}
