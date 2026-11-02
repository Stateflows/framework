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
        private readonly EventHolder? executionTriggerHolder;

        public BaseContext(RootContext context, NodeScope nodeScope)
            : base(context.Context, nodeScope.ServiceProvider, nodeScope.CancellationTokenSource)
        {
            Context = context;
            NodeScope = nodeScope;
            executionTriggerHolder = context.ExecutionTriggerHolder;
            ExecutionMode = context.IsStreamingExecution ? ExecutionMode.Streamed : ExecutionMode.Transactional;
            nodeScope.TrackContext(this);
        }

        public BaseContext(BaseContext context)
            : base(context.Context.Context, context.ServiceProvider, context.NodeScope.CancellationTokenSource)
        {
            Context = context.Context;
            NodeScope = context.NodeScope;
            executionTriggerHolder = context.executionTriggerHolder;
            ExecutionMode = context.ExecutionMode;
            NodeScope.TrackContext(this);
        }

        public IServiceProvider ServiceProvider
            => NodeScope.ServiceProvider;
        
        public RootContext Context { get; }

        public NodeScope NodeScope { get; }

        public virtual ExecutionMode ExecutionMode { get; }
        public object? ExecutionTrigger => ExecutionMode == ExecutionMode.Streamed ? null : executionTriggerHolder?.BoxedPayload;
        public Guid? ExecutionTriggerId => ExecutionMode == ExecutionMode.Streamed ? null : executionTriggerHolder?.Id;
        public IDictionary<string, EventHeader>? Headers => ExecutionMode == ExecutionMode.Streamed ? null : executionTriggerHolder?.Headers;

        private ActivityContext? activity;
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
