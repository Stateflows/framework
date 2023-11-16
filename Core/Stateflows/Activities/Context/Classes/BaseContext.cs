﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Context.Classes
{
    internal class BaseContext
    {
        public BaseContext(RootContext context, NodeScope nodeScope)
        {
            Context = context;
            NodeScope = nodeScope;
        }

        public IServiceProvider ServiceProvider => NodeScope.ServiceProvider;

        public RootContext Context { get; }

        public NodeScope NodeScope { get; }

        public ActivityContext activity;
        public ActivityContext Activity => activity ??= new ActivityContext(Context, NodeScope);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ??= NodeScope.ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);
    }
}