using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Classes
{
    internal class BaseContext : IStateflowsContextProvider, IBehaviorLocator
    {
        public BaseContext(RootContext context)
        {
            Context = context;
        }

        public RootContext Context { get; }

        public object ExecutionTrigger => Context.ExecutionTriggerHolder.BoxedPayload;
        public Guid ExecutionTriggerId => Context.ExecutionTriggerHolder.Id;
        public IEnumerable<EventHeader> Headers => Context.ExecutionTriggerHolder.Headers;

        public IEnumerable<IExecutionStep> ExecutionSteps => Context.ExecutionSteps;

        public StateMachineContext stateMachine;
        public StateMachineContext StateMachine => stateMachine ??= new StateMachineContext(Context);

        private IBehaviorLocator behaviorLocator;
        private IBehaviorLocator BehaviorLocator => behaviorLocator ??= Context.Executor.ServiceProvider.GetService<IBehaviorLocator>();

        public bool TryLocateBehavior(BehaviorId id, out IBehavior behavior)
            => BehaviorLocator.TryLocateBehavior(id, out behavior);

        StateflowsContext IStateflowsContextProvider.Context => Context.Context;
    }
}
