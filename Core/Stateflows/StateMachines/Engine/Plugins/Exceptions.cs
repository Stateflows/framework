using System;
using System.Collections.Generic;
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
        
        private bool HandleException(StateMachineId stateMachineId, Exception exception, IEnumerable<EventHeader> headers)
        {
            if (locator.TryLocateStateMachine(stateMachineId, out var stateMachine))
            {
                _ = stateMachine.SendAsync(exception, headers);
            }

            return true;
        }
        
        public override bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnStateInitializationException(IStateActionContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnStateFinalizationException(IStateActionContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnStateEntryException(IStateActionContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);

        public override bool OnStateExitException(IStateActionContext context, Exception exception)
            => HandleException(context.Behavior.Id, exception, context.Headers);
    }
}
