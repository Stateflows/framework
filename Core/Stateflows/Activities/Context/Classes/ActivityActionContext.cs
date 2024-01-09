using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityActionContext : BaseContext,
        IActivityActionInspectionContext,
        IActivityFinalizationInspectionContext,
        IRootContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IActivityActionInspectionContext.Activity => Activity;

        IActivityInspectionContext IActivityFinalizationInspectionContext.Activity => Activity;

        public ActivityActionContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }

        public ActivityActionContext(BaseContext context)
            : base(context)
        { }
    }
}
