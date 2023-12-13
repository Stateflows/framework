using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class AcceptEventAction<TEvent> : Action
        where TEvent : Event, new()
    {
        public new IAcceptEventActionContext<TEvent> Context { get; internal set; }
    }
}
