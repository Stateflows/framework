using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IAcceptEventActionContext<out TEvent> : IActionContext
        where TEvent : Event
    {
        TEvent Event { get; }
    }
}
