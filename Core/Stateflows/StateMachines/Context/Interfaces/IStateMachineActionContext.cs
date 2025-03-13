using System;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineActionContext : IExecutionContext, IBehaviorActionContext
    {
        [Obsolete("StateMachine context property is obsolete, use Behavior or CurrentState properties instead.")]
        IStateMachineContext StateMachine { get; }
        
        /// <summary>
        /// Information about current state of a State Machine
        /// </summary>
        IReadOnlyTree<IStateContext> CurrentState => StateMachine.CurrentState;

        /// <summary>
        /// Information about current behavior
        /// </summary>
        IBehaviorContext Behavior => StateMachine;
    }
}
