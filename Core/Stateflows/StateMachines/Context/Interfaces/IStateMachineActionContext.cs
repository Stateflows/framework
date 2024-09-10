using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IInitializationContext
    {
        EventHolder InitializationEvent { get; }
    }

    public interface IStateMachineActionContext : IBehaviorLocator, IExecutionContext
    {
        IStateMachineContext StateMachine { get; }
    }
}
