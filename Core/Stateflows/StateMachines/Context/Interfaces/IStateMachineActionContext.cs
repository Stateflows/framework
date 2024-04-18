using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineActionContext : IBehaviorLocator
    {
        IStateMachineContext StateMachine { get; }

        object ExecutionTrigger { get; }
    }
}
