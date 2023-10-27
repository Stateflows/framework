using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventInspectionContext<TEvent>, IRootContext
        where TEvent : Event
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IEventInspectionContext<TEvent>.Activity => Activity;

        public EventContext(RootContext context, NodeScope nodeScope, TEvent @event)
            : base(context, nodeScope)
        {
            Event = @event;
        }

        public TEvent Event { get; set; }
    }
}
