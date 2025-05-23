using System;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineActionContext : IExecutionContext, IBehaviorActionContext
    {
        [Obsolete("StateMachine context property is obsolete, use Behavior or CurrentState properties instead.")]
        IStateMachineContext StateMachine { get; }
        
        /// <summary>
        /// Information about current state of a State Machine
        /// </summary>
        IReadOnlyTree<IStateContext> CurrentStates => StateMachine.CurrentStates;

        /// <summary>
        /// Information about current behavior
        /// </summary>
        new IBehaviorContext Behavior => StateMachine;
    }
}
