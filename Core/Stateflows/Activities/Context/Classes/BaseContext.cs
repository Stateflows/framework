﻿using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Activities.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Context;

namespace Stateflows.Activities.Context.Classes
{
    internal class BaseContext : IStateflowsContextProvider
    {
        public BaseContext(RootContext context, NodeScope nodeScope)
        {
            Context = context;
            NodeScope = nodeScope;
        }

        public BaseContext(BaseContext context)
        {
            Context = context.Context;
            NodeScope = context.NodeScope;
        }

        public IServiceProvider ServiceProvider
            => NodeScope.ServiceProvider;

        public RootContext Context { get; }

        public NodeScope NodeScope { get; }

        public CancellationToken CancellationToken => NodeScope.CancellationToken;

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
