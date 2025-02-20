using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Context;

namespace Stateflows.Actions.Context.Classes
{
    public class ActionDelegateContext : IActionDelegateContext
    {
        public ActionDelegateContext(StateflowsContext context, IServiceProvider serviceProvider, List<TokenHolder> inputTokens = null)
        {
            ServiceProvider = serviceProvider;
            RootContext = new RootContext(context);
            InputTokens = inputTokens;
        }

        public readonly IServiceProvider ServiceProvider;
        private readonly RootContext RootContext;
        public readonly List<TokenHolder> InputTokens;

        public IEnumerable<TokenHolder> OutputTokens
            => Action.OutputTokens;

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator
            => behaviorLocator ??= ServiceProvider.GetService<IBehaviorLocator>();
        
        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);

        public object ExecutionTrigger
            => RootContext.EventHolder.BoxedPayload;

        private ActionContext action;
        private ActionContext Action
            => action ??= new ActionContext(RootContext, ServiceProvider, InputTokens);
        
        IActionContext IActionDelegateContext.Action => Action;
    }
}