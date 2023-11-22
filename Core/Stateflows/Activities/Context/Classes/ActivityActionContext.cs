using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityActionContext : BaseContext, IActivityActionContext, IActivityActionInspectionContext
    {
        IActivityContext IActivityActionContext.Activity => Activity;

        IActivityInspectionContext IActivityActionInspectionContext.Activity => Activity;

        public ActivityActionContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }
    }
}
