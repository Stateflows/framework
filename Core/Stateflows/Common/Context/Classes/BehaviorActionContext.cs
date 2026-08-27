using System;
using System.Collections.Generic;

namespace Stateflows.Common.Context.Classes
{
    internal class BehaviorActionContext(StateflowsContext context, IServiceProvider serviceProvider) :
        BaseContext(context, serviceProvider),
        IBehaviorActionContext
    {
        IBehaviorContext IBehaviorActionContext.Behavior => Behavior;
        // IParentBehaviorContext IBehaviorActionContext.ParentBehavior => Behavior;
        // IOwnerBehaviorContext IBehaviorActionContext.OwnerBehavior => Behavior;
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

        // todo
        public virtual object ExecutionTrigger { get; init; }
        public Guid ExecutionTriggerId { get; init; }
        public virtual IDictionary<string, EventHeader> Headers { get; init; }
    }
}
