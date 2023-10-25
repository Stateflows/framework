using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Context.Classes
{
    internal class BaseContext : IBehaviorLocator
    {
        public BaseContext(StateflowsContext context, IServiceProvider serviceProvider)
        {
            Context = context;
            ServiceProvider = serviceProvider;
        }

        public StateflowsContext Context { get; }

        public IServiceProvider ServiceProvider { get; }

        public BehaviorContext behavior;
        public BehaviorContext Behavior => behavior ??= new BehaviorContext(Context, ServiceProvider);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ??= ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);
    }
}
