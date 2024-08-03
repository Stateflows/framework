using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityInitializationContext<TInitializationEvent> :
        BaseContext,
        IActivityInitializationContext<TInitializationEvent>,
        IRootContext
        where TInitializationEvent : Event, new()
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, TInitializationEvent initializationEvent)
            : base(context, nodeScope)
        {
            InitializationEvent = initializationEvent;
        }

        public ActivityInitializationContext(BaseContext context, TInitializationEvent initializationEvent)
            : base(context)
        {
            InitializationEvent = initializationEvent;
        }

        IActivityContext IActivityActionContext.Activity => Activity;

        public TInitializationEvent InitializationEvent { get; }
    }

    internal class ActivityInitializationContext :
        ActivityInitializationContext<Event>,
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(BaseContext context, Event initializationEvent)
            : base(context, initializationEvent)
        { }

        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, Event initializationEvent)
            : base(context, nodeScope, initializationEvent)
        { }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;
    }
}
