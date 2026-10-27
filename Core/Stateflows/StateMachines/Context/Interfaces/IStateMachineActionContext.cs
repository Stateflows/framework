using Stateflows.Common;

namespace Stateflows.StateMachines
{
    /// <summary>Provides behavior services, execution data, and state configuration to state-machine callbacks.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/behavior-values">behavior and state values</see>.</remarks>
    public interface IStateMachineActionContext : IExecutionContext, IBehaviorActionContext
    {
        /// <summary>
        /// Information about current state of a State Machine
        /// </summary>
        IReadOnlyTree<IStateContext> CurrentStates { get; }
        
        /// <summary>Attempts to resolve a defined state independently of its active status.</summary>
        /// <param name="stateName">The registered state name.</param>
        /// <param name="stateContext">The matching context when the definition contains this state.</param>
        /// <returns>Whether the named state exists in the machine definition.</returns>
        bool TryGetStateContext(string stateName, out IStateContext stateContext);

        // /// <summary>
        // /// Information about current behavior
        // /// </summary>
        // new IBehaviorContext Behavior { get; }
    }
}
