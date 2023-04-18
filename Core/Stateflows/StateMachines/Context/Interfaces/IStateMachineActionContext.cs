using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineActionContext : IBehaviorLocator
    {
        IStateMachineContext StateMachine { get; }
    }
}
