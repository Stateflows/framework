using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Provides asynchronous access to values associated with a behavior execution context.</summary>
    public interface IContextValues
    {
        /// <summary>Stores a value under a key and returns the stored value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The storage key.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>The stored value.</returns>
        Task<T> SetAsync<T>(string key, T value);

        /// <summary>Determines whether a value exists for a key.</summary>
        /// <param name="key">The storage key.</param>
        /// <returns><see langword="true"/> when a value has been stored for the key.</returns>
        Task<bool> IsSetAsync(string key);

        /// <summary>Determines whether any stored keys begin with a prefix.</summary>
        /// <param name="prefix">The key prefix.</param>
        /// <returns><see langword="true"/> when at least one key has the prefix.</returns>
        Task<bool> HasAnyPrefixedAsync(string prefix);

        /// <summary>Attempts to retrieve a value using the requested type.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="key">The storage key.</param>
        /// <returns>A success flag and the retrieved value, or a default value when absent or incompatible.</returns>
        Task<(bool Success, T? Value)> TryGetAsync<T>(string key);

        /// <summary>Gets a stored value or returns the supplied default.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="key">The storage key.</param>
        /// <param name="defaultValue">The value returned when no stored value is available.</param>
        /// <returns>The stored value or <paramref name="defaultValue"/>.</returns>
        Task<T?> GetOrDefaultAsync<T>(string key, T? defaultValue = default);

        /// <summary>Atomically updates a value using a callback.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The storage key.</param>
        /// <param name="valueUpdater">Computes the replacement value from the current value.</param>
        /// <param name="defaultValue">The value supplied to the callback when no value is stored.</param>
        /// <returns>The value produced by the update callback.</returns>
        Task<T?> UpdateAsync<T>(string key, Func<T?, T?> valueUpdater, T? defaultValue = default);

        /// <summary>Removes the value stored under a key.</summary>
        /// <param name="key">The storage key.</param>
        Task RemoveAsync(string key);
        
        /// <summary>Removes all values whose keys begin with a prefix.</summary>
        /// <param name="prefix">The key prefix.</param>
        Task RemovePrefixedAsync(string prefix);

        /// <summary>Removes all values in this context.</summary>
        Task ClearAsync();
    }
}
