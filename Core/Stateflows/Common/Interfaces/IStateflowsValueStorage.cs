using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Defines persistent, per-behavior storage for named values.</summary>
    /// <remarks>Implementations are registered as the host's value-storage service and must be disposed when their lifetime ends.</remarks>
    public interface IStateflowsValueStorage : IDisposable
    {
        /// <summary>Stores or replaces a typed value for a behavior.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>The stored value.</returns>
        Task<T> SetAsync<T>(BehaviorId behaviorId, string key, T value);

        /// <summary>Determines whether a value is stored for a behavior and key.</summary>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key.</param>
        /// <returns><see langword="true"/> when the value exists.</returns>
        Task<bool> IsSetAsync(BehaviorId behaviorId, string key);

        /// <summary>Determines whether a behavior has any values with keys beginning with a prefix.</summary>
        /// <param name="behaviorId">The behavior instance owning the values.</param>
        /// <param name="prefix">The key prefix.</param>
        /// <returns><see langword="true"/> when at least one key has the prefix.</returns>
        Task<bool> HasAnyPrefixedAsync(BehaviorId behaviorId, string prefix);

        /// <summary>Attempts to retrieve a value with the requested type.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key.</param>
        /// <returns>A success flag and the retrieved value when present and compatible.</returns>
        Task<(bool Success, T? Value)> TryGetAsync<T>(BehaviorId behaviorId, string key);

        /// <summary>Gets a stored value or returns a default.</summary>
        /// <typeparam name="T">The expected value type.</typeparam>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key.</param>
        /// <param name="defaultValue">The value returned when no stored value is available.</param>
        /// <returns>The stored or default value.</returns>
        Task<T> GetOrDefaultAsync<T>(BehaviorId behaviorId, string key, T defaultValue = default);

        /// <summary>Atomically updates a value using a callback.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key.</param>
        /// <param name="valueUpdater">Computes the replacement from the current value.</param>
        /// <param name="defaultValue">The value supplied to the callback when none is stored.</param>
        /// <returns>The value produced by the update callback.</returns>
        Task<T> UpdateAsync<T>(BehaviorId behaviorId, string key, Func<T, T> valueUpdater, T defaultValue = default);

        /// <summary>Removes a value from a behavior's value store.</summary>
        /// <param name="behaviorId">The behavior instance owning the value.</param>
        /// <param name="key">The value key to remove.</param>
        Task RemoveAsync(BehaviorId behaviorId, string key);

        /// <summary>Removes all values whose keys begin with a prefix.</summary>
        /// <param name="behaviorId">The behavior instance owning the values.</param>
        /// <param name="prefix">The key prefix.</param>
        Task RemovePrefixedAsync(BehaviorId behaviorId, string prefix);

        /// <summary>Removes every stored value for a behavior.</summary>
        /// <param name="behaviorId">The behavior instance whose values are cleared.</param>
        Task ClearAsync(BehaviorId behaviorId);
    }
}