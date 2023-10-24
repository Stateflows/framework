using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class BaseContext
    {
        public BaseContext(RootContext context)
        {
            Context = context;
        }
        //public IServiceProvider ServiceProvider => Context.Executor.ServiceProvider;

        public RootContext Context { get; }

        public StateMachineContext stateMachine;
        public StateMachineContext StateMachine => stateMachine ??= new StateMachineContext(Context);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ??= Context.Executor.ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);

        public IEnumerable<IBehaviorProvider> Providers
            => BehaviorLocator.Providers;
    }
}
