using System;
using System.Collections.Generic;
using System.Threading;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;

namespace Stateflows.Entities.Context.Classes
{
    /// <summary>
    /// Lightweight, non-disposable behavior action context shared by all entity
    /// observation and interception contexts of a single execution.
    /// </summary>
    internal sealed class EntityActionContext(BehaviorContext behaviorContext) : IBehaviorActionContext
    {
        public BehaviorContext BehaviorContext { get; } = behaviorContext;

        public IBehaviorContext Behavior => BehaviorContext;

        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = BehaviorContext.Context.ContextParentId.HasValue
                ? BehaviorContext
                : null!;

            return parentBehaviorContext != null!;
        }

        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = BehaviorContext.Context.ContextOwnerId.HasValue
                ? BehaviorContext
                : null!;

            return ownerBehaviorContext != null!;
        }

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorContext.TryLocateBehavior(id, out behavior);

        public object ExecutionTrigger => BehaviorContext.Context.ExecutionTriggerHolder?.BoxedPayload!;

        public Guid ExecutionTriggerId => BehaviorContext.Context.ExecutionTriggerHolder?.Id ?? Guid.Empty;

        public IDictionary<string, EventHeader> Headers
            => BehaviorContext.Context.ExecutionTriggerHolder?.Headers ?? new Dictionary<string, EventHeader>();

        public CancellationToken CancellationToken => BehaviorContext.CancellationToken;
    }
}