using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public abstract class EventAction<TEvent> : ActivityNode
        where TEvent : Event
    {
        new public IAcceptEventActionContext<TEvent> Context { get; internal set; }
    }

    public static class EventActionInfo<TEvent, TEventAcceptAction>
        where TEvent : Event
        where TEventAcceptAction : EventAction<TEvent>
    {
        public static string Name => typeof(TEventAcceptAction).FullName;
    }
}
