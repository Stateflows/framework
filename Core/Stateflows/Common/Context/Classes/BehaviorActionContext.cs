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

        public virtual ExecutionMode ExecutionMode { get; init; } = context.ExecutionMode;
        public virtual object? ExecutionTrigger { get; init; }
            = context.ExecutionMode == ExecutionMode.Streamed ? null : context.ExecutionTriggerHolder?.BoxedPayload;
        public virtual Guid? ExecutionTriggerId { get; init; }
            = context.ExecutionMode == ExecutionMode.Streamed ? null : context.ExecutionTriggerHolder?.Id;
        public virtual IDictionary<string, EventHeader>? Headers { get; init; }
            = context.ExecutionMode == ExecutionMode.Streamed ? null : context.ExecutionTriggerHolder?.Headers;
    }
}
