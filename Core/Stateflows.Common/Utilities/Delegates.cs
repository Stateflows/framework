using System.Threading.Tasks;

namespace Stateflows.Common.Utilities
{
    /// <summary>Performs an asynchronous operation without an input payload.</summary>
    /// <returns>A task that completes when the operation finishes.</returns>
    public delegate Task ActionAsync();

    /// <summary>Performs an asynchronous operation for an input value.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <param name="param">The value supplied to the operation.</param>
    /// <returns>A task that completes when the operation finishes.</returns>
    public delegate Task ActionAsync<in T>(T param);

    /// <summary>Asynchronously evaluates a condition without an input payload.</summary>
    /// <returns>A task containing the condition result.</returns>
    public delegate Task<bool> PredicateAsync();

    /// <summary>Asynchronously evaluates a condition for an input value.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <param name="param">The value to evaluate.</param>
    /// <returns>A task containing the condition result.</returns>
    public delegate Task<bool> PredicateAsync<in T>(T param);
}
