using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent>
    {
        TEvent Event { get; }
    }

    public interface IEventActionContext<out TEvent> : IEventContext<TEvent>, IStateMachineActionContext
    { }
}
