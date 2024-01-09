using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityInitializationContext<TInitializationRequest> :
        BaseContext,
        IActivityInitializationContext<TInitializationRequest>,
        IRootContext
        where TInitializationRequest : InitializationRequest
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, TInitializationRequest initializationRequest)
            : base(context, nodeScope)
        {
            InitializationRequest = initializationRequest;
        }

        public ActivityInitializationContext(BaseContext context, TInitializationRequest initializationRequest)
            : base(context)
        {
            InitializationRequest = initializationRequest;
        }

        IActivityContext IActivityActionContext.Activity => Activity;

        public TInitializationRequest InitializationRequest { get; }
    }

    internal class ActivityInitializationContext :
        ActivityInitializationContext<InitializationRequest>,
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(BaseContext context, InitializationRequest initializationRequest)
            : base(context, initializationRequest)
        { }

        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, InitializationRequest initializationRequest)
            : base(context, nodeScope, initializationRequest)
        { }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;
    }
}
