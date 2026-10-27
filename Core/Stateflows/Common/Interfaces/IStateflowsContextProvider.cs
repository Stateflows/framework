using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Provides access to the context associated with the current execution scope.</summary>
    public interface IStateflowsContextProvider
    {
        /// <summary>Gets the current behavior execution context.</summary>
        public StateflowsContext Context { get; }
    }
}
