using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Provides the identity, persistent values, and containment of a state in a machine instance.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behavior-values">behavior and state values</see>.</remarks>
    public interface IStateContext
    {
        /// <summary>The registered state name, rather than its display label or CLR type name.</summary>
        string Name { get; }

        /// <summary>Values scoped to this state in the current machine instance.</summary>
        IContextValues Values { get; }

        /// <summary>Attempts to locate the enclosing composite or orthogonal state.</summary>
        /// <param name="parentStateContext">The enclosing state's context when one exists.</param>
        /// <returns><see langword="true"/> for a nested state; otherwise <see langword="false"/>.</returns>
        bool TryGetParent(out IStateContext parentStateContext);
    }
}
