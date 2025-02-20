using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IContextValues
    {
        [Obsolete("Method Set() is deprecated and will be removed soon. Use SetAsync() instead")]
        void Set<T>(string key, T value);

        [Obsolete("Method IsSet() is deprecated and will be removed soon. Use IsSetAsync() instead")]
        bool IsSet(string key);

        [Obsolete("Method TryGet() is deprecated and will be removed soon. Use TryGetAsync() instead")]
        bool TryGet<T>(string key, out T value);

        [Obsolete("Method GetOrDefault() is deprecated and will be removed soon. Use GetOrDefaultAsync() instead")]
        T GetOrDefault<T>(string key, T defaultValue = default);

        [Obsolete("Method Update() is deprecated and will be removed soon. Use UpdateAsync() instead")]
        void Update<T>(string key, Func<T, T> valueUpdater, T defaultValue = default);

        [Obsolete("Method Remove() is deprecated and will be removed soon. Use RemoveAsync() instead")]
        void Remove(string key);

        [Obsolete("Method Clear() is deprecated and will be removed soon. Use ClearAsync() instead")]
        void Clear();
        
        Task SetAsync<T>(string key, T value);

        Task<bool> IsSetAsync(string key);

        Task<(bool Success, T Value)> TryGetAsync<T>(string key);

        Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default);

        Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default);

        Task RemoveAsync(string key);

        Task ClearAsync();
    }
}
