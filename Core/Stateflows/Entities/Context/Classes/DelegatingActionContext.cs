using System;
using System.Collections.Generic;
using System.Threading;
using Stateflows.Common;

namespace Stateflows.Entities.Context.Classes
{
    internal abstract class DelegatingActionContext(EntityActionContext actionContext) : IBehaviorActionContext
    {
        protected EntityActionContext ActionContext { get; } = actionContext;

        public IBehaviorContext Behavior => ActionContext.Behavior;

        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
            => ActionContext.TryGetParentBehaviorContext(out parentBehaviorContext);

        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
            => ActionContext.TryGetOwnerBehaviorContext(out ownerBehaviorContext);

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => ActionContext.TryLocateBehavior(id, out behavior);

        public virtual object ExecutionTrigger => ActionContext.ExecutionTrigger;

        public virtual Guid ExecutionTriggerId => ActionContext.ExecutionTriggerId;

        public virtual IDictionary<string, EventHeader> Headers => ActionContext.Headers;

        public CancellationToken CancellationToken => ActionContext.CancellationToken;
    }
}