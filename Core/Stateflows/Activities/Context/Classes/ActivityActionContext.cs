using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class ActivityActionContext : BaseContext,
        IActivityFinalizationContext,
        IRootContext
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Activity;

        public ActivityActionContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
