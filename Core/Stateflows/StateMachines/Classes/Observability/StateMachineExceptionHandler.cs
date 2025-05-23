using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineExceptionHandler : IStateMachineExceptionHandler
    {
        public virtual bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
                => false;

        public virtual bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
            => false;

        public virtual bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => false;

        public virtual bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => false;

        public virtual bool OnStateInitializationException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateFinalizationException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateEntryException(IStateActionContext context, Exception exception)
            => false;

        public virtual bool OnStateExitException(IStateActionContext context, Exception exception)
            => false;
    }
}