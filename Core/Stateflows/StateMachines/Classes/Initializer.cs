using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class Initializer<TInitializationEvent> : BaseInitializer, IInitializer<TInitializationEvent>
        where TInitializationEvent : Event, new()
    {
        [Obsolete("Context is deprecated, use dependency injection services (IExecutionContext, IInitializationContext, IStateMachineContext, IBehaviorLocator) or value accesors (GlobalValue)")]
        new public IStateMachineInitializationContext<TInitializationEvent> Context
            => (IStateMachineInitializationContext<TInitializationEvent>)base.Context;

        public abstract Task<bool> OnInitialize(TInitializationEvent initializationEvent);
    }
}
