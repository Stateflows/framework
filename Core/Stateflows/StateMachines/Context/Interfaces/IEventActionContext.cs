using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent>
        where TEvent : Event, new()
    {
        TEvent Event { get; }
    }

    public interface IEventActionContext<out TEvent> : IEventContext<TEvent>, IStateMachineActionContext
        where TEvent : Event, new()
    { }
}
