using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Defines asynchronous storage for string values indexed by key.</summary>
    public interface IStateflowsCache
    {
        /// <summary>Stores or replaces a value for a key.</summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to store.</param>
        Task SetAsync(string key, string value);
        /// <summary>Updates a value by applying a callback to its current or default value.</summary>
        /// <param name="key">The cache key.</param>
        /// <param name="valueUpdater">The callback that computes the replacement value.</param>
        /// <param name="defaultValue">The value supplied to the callback when the key is absent.</param>
        Task UpdateAsync(string key, Func<string, string> valueUpdater, string defaultValue = null);
        /// <summary>Attempts to retrieve a value by key.</summary>
        /// <param name="key">The cache key.</param>
        /// <returns>A success flag and the retrieved value when present.</returns>
        Task<(bool Success, string Value)> TryGetAsync(string key);
        /// <summary>Removes a value by key.</summary>
        /// <param name="key">The cache key to remove.</param>
        Task RemoveAsync(string key);
    }
}
