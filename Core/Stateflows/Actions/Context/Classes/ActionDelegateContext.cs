using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Activities;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Engine;

namespace Stateflows.Actions.Context.Classes
{
    internal class ActionDelegateContext : ActionContext, IActionDelegateContext
    {
        internal static Dictionary<BehaviorId, List<ActionDelegateContext>> Instances = [];

        public ActionDelegateContext(StateflowsContext context, Executor executor, EventHolder eventHolder,
            IServiceProvider serviceProvider, List<TokenHolder> inputTokens = null)
            : base(new RootContext(context, executor, eventHolder, serviceProvider), serviceProvider, inputTokens)
        {
            lock (Instances)
            {
                if (!Instances.TryGetValue(Context.Id, out var contextList))
                {
                    contextList = [];
                    Instances.Add(Context.Id, contextList);
                }
                
                contextList.Add(this);
            }
        }

        public object ExecutionTrigger => RootContext.EventHolder.BoxedPayload;
        public Guid ExecutionTriggerId => RootContext.EventHolder.Id;
        public List<EventHeader> Headers => RootContext.EventHolder.Headers;
        public IBehaviorContext Behavior => this;

        public void Clear()
        {
            lock (Instances)
            {
                if (!Instances.TryGetValue(Context.Id, out var contextList))
                {
                    contextList = [];
                    Instances.Add(Context.Id, contextList);
                }
                
                contextList.Remove(this);
            }
        }
    }
}