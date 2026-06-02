using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Engine;

namespace Stateflows.Activities.Context.Classes
{
    internal class BaseContext : Stateflows.Common.Context.Classes.BaseContext, IStateflowsContextProvider, IBehaviorLocator
    {
        public BaseContext(RootContext context, NodeScope nodeScope)
            : base(context.Context, nodeScope.ServiceProvider)
        {
            Context = context;
            NodeScope = nodeScope;
        }

        public BaseContext(BaseContext context)
            : base(context.Context.Context, context.ServiceProvider)
        {
            Context = context.Context;
            NodeScope = context.NodeScope;
            CancellationTokenSource = NodeScope.CancellationTokenSource;
        }

        public IServiceProvider ServiceProvider
            => NodeScope.ServiceProvider;
        
        public RootContext Context { get; }

        public NodeScope NodeScope { get; }

        public object ExecutionTrigger => Context.ExecutionTriggerHolder.BoxedPayload;
        public Guid ExecutionTriggerId => Context.ExecutionTriggerHolder.Id;
        public Dictionary<string, EventHeader> Headers => Context.ExecutionTriggerHolder.Headers;

        private ActivityContext activity;
        public ActivityContext Activity
            => activity ??= new ActivityContext(Context, NodeScope);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator
            => behaviorLocator ??= NodeScope.ServiceProvider.GetService<IBehaviorLocator>();

        StateflowsContext IStateflowsContextProvider.Context => Context.Context;

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);
    }
}
