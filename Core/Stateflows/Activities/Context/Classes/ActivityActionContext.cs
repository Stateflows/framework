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
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Behavior.Context.ContextParentId.HasValue
                ? Behavior
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Behavior.Context.ContextOwnerId.HasValue
                ? Behavior
                : null;
            
            return ownerBehaviorContext != null;
        }

        public ActivityActionContext(RootContext context, NodeScope nodeScope)
            : base(context, nodeScope)
        { }

        public object LockHandle => Activity.LockHandle;
        public IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
