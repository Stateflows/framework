using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineInitializationContext : IStateMachineActionContext
    { }

    public interface IStateMachineInitializationContext<out TInitializationEvent> : IStateMachineInitializationContext
        where TInitializationEvent : Event, new()
    {
        TInitializationEvent InitializationEvent { get; }
    }
}
