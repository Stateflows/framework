using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Engine;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityEventActionContext : ActivityActionContext, IActivityEventActionContext//, IActivityActionInspectionContext
    {
        public Event Event { get; set; }

        public ActivityEventActionContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }
    }
}
