using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IStateMachineActionContext
        where TEvent : Event, new()
    {
        TEvent Event { get; }
    }
}
