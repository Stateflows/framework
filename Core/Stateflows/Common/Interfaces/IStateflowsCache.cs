using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsCache
    {
        Task SetAsync(string key, string value);
        Task UpdateAsync(string key, Func<string, string> valueUpdater, string defaultValue = null);
        Task<(bool Success, string Value)> TryGetAsync(string key);
        Task RemoveAsync(string key);
    }
}
