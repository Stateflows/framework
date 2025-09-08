using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IContextValues
    {
        Task SetAsync<T>(string key, T value);

        Task<bool> IsSetAsync(string key);

        Task<bool> HasAnyPrefixedAsync(string prefix);

        Task<(bool Success, T Value)> TryGetAsync<T>(string key);

        Task<T> GetOrDefaultAsync<T>(string key, T defaultValue = default);

        Task UpdateAsync<T>(string key, Func<T, T> valueUpdater, T defaultValue = default);

        Task RemoveAsync(string key);
        
        Task RemovePrefixedAsync(string prefix);

        Task ClearAsync();
    }
}
