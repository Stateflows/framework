using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Provides asynchronous storage operations for one typed behavior value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    public interface IValue<T>
    {
        /// <summary>Stores or replaces the value.</summary>
        /// <param name="value">The value to store.</param>
        Task SetAsync(T value);

        /// <summary>Determines whether a value is currently stored.</summary>
        /// <returns><see langword="true"/> when a value is stored.</returns>
        Task<bool> IsSetAsync();

        /// <summary>Attempts to retrieve the stored value.</summary>
        /// <returns>A success flag and the value when present.</returns>
        Task<(bool Success, T Value)> TryGetAsync();

        /// <summary>Gets the stored value or returns a default.</summary>
        /// <param name="defaultValue">The value to return when none is stored.</param>
        /// <returns>The stored or default value.</returns>
        Task<T> GetOrDefaultAsync(T defaultValue = default);

        /// <summary>Updates the value using the supplied function, storing the result.</summary>
        /// <param name="valueUpdater">Computes a replacement from the current value.</param>
        /// <param name="defaultValue">The value supplied to the updater when none is stored.</param>
        Task UpdateAsync(Func<T, T> valueUpdater, T defaultValue = default);

        /// <summary>Removes the stored value.</summary>
        Task RemoveAsync();
    }
}
