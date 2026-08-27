using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Engine;

namespace Stateflows.Actions.Context.Classes
{
    internal class ActionDelegateContext : ActionContext, IActionDelegateContext
    {
        public ActionDelegateContext(StateflowsContext context, Executor executor, EventHolder eventHolder,
            IServiceProvider serviceProvider, List<TokenHolder> inputTokens = null)
            : base(new RootContext(context, executor, eventHolder, serviceProvider), serviceProvider, inputTokens)
        { }

        public override object ExecutionTrigger => RootContext.EventHolder.BoxedPayload;
        public Guid ExecutionTriggerId => RootContext.EventHolder.Id;
        public override Dictionary<string, EventHeader> Headers => RootContext.EventHolder.Headers;
        public IBehaviorContext Behavior => this;
        public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        {
            parentBehaviorContext = Context.ContextParentId.HasValue
                ? this
                : null;
            
            return parentBehaviorContext != null;
        }
        public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        {
            ownerBehaviorContext = Context.ContextParentId.HasValue
                ? this
                : null;
            
            return ownerBehaviorContext != null;
        }
    }
}