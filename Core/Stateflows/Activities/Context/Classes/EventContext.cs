using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Context;

namespace Stateflows.Activities.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventInspectionContext<TEvent>, IRootContext
        where TEvent : Event
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IEventInspectionContext<TEvent>.Activity => Activity;

        public EventContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }

        public TEvent Event => Context.Event as TEvent;
    }
}
