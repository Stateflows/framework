using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;
using Stateflows.Activities.Engine;

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

        IActivityContext IActivityActionContext.Activity => Activity;

        public TInitializationRequest InitializationRequest { get; }
    }

    internal class ActivityInitializationContext :
        ActivityInitializationContext<InitializationRequest>,
        IActivityInitializationContext,
        IActivityInitializationInspectionContext,
        IRootContext
    {
        public ActivityInitializationContext(RootContext context, NodeScope nodeScope, InitializationRequest initializationRequest)
            : base(context, nodeScope, initializationRequest)
        { }

        IActivityInspectionContext IActivityInitializationInspectionContext.Activity => Activity;
    }
}
