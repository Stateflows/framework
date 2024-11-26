using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineActionContext : IBehaviorLocator, IExecutionContext
    {
        IStateMachineContext StateMachine { get; }
    }
}
