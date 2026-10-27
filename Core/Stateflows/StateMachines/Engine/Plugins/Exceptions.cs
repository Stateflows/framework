using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal class Exceptions : StateMachinePlugin
    {
        private readonly IStateMachineLocator locator;
        public Exceptions(IStateMachineLocator locator)
        {
            this.locator = locator;
        }
        
        private bool HandleException(StateMachineId stateMachineId, Exception exception, IDictionary<string, EventHeader> headers)
        {
            if (locator.TryLocateStateMachine(stateMachineId, out var stateMachine))
            {
                _ = stateMachine.SendAsync(exception, headers.ToDictionary());
            }

            return true;
        }
        
        public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));

        public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(HandleException(context.Behavior.Id, exception, context.Headers));
    }
}
