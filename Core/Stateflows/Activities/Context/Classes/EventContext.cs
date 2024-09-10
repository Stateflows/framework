using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class EventContext<TEvent> : BaseContext, IEventInspectionContext<TEvent>, IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IEventInspectionContext<TEvent>.Activity => Activity;

        public EventContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }

        public TEvent Event => (Context.EventHolder as EventHolder<TEvent>).Payload;
    }
}
