using System.Linq;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class AcceptEventActionContext<TEvent> : ActionContext, IAcceptEventActionContext<TEvent>
        where TEvent : Event, new()
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        public TEvent @event = null;
        public TEvent Event => @event ??= Input.OfType<EventToken<TEvent>>().First(t => t.Event is TEvent).Event;

        public AcceptEventActionContext(ActionContext actionContext)
            : base(actionContext.Context, actionContext.NodeScope, actionContext.Node, actionContext.Input)
        { }
    }
}
