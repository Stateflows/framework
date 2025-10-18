using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineActionContext : IExecutionContext, IBehaviorActionContext
    {
        /// <summary>
        /// Information about current state of a State Machine
        /// </summary>
        IReadOnlyTree<IStateContext> CurrentStates { get; }
        
        bool TryGetStateContext(string stateName, out IStateContext stateContext);

        // /// <summary>
        // /// Information about current behavior
        // /// </summary>
        // new IBehaviorContext Behavior { get; }
    }
}
