using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IActivityActionContext
        where TEvent : Event, new()
    {
        TEvent Event { get; }
    }
}
