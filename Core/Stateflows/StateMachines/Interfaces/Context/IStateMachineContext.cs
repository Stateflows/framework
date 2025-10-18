using Stateflows.Common;

namespace Stateflows.StateMachines
{
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
        
        bool TryGetStateContext(string stateName, out IStateContext stateContext);
    }
}
