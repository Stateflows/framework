using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Exposes a state-machine instance and its current active-state configuration.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behavior-values">behavior and state values</see>.</remarks>
    public interface IStateMachineContext : IBehaviorContext
    {
        /// <summary>
        /// Identifier of current State Machine behavior instance
        /// </summary>
        new StateMachineId Id { get; }
        
        /// <summary>
        /// Tree of States that represents current configuration of State Machine behavior instance
        /// </summary>
        IReadOnlyTree<IStateContext> CurrentStates { get; }
        
        /// <summary>Attempts to resolve a state context by its registered name.</summary>
        /// <param name="stateName">The registered name of the state.</param>
        /// <param name="stateContext">The resolved context when the state exists.</param>
        /// <returns><see langword="true"/> when a state with this name exists, regardless of whether it is active.</returns>
        bool TryGetStateContext(string stateName, out IStateContext stateContext);
    }
}
