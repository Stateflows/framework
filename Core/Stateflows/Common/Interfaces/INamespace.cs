using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Provides access to values within a named hierarchical namespace.</summary>
    public interface INamespace
    {
        /// <summary>Gets a typed value handle for a key in this namespace.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="key">The key of the value.</param>
        /// <returns>A handle for reading or changing the value.</returns>
        IValue<T> GetValue<T>(string key);
        
        /// <summary>Gets a child namespace.</summary>
        /// <param name="namespaceName">The child namespace name.</param>
        /// <returns>The requested child namespace.</returns>
        INamespace GetNamespace(string namespaceName);

        /// <summary>Clears the values in this namespace.</summary>
        Task ClearAsync();
    }
}
