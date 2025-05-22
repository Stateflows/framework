using System;
using Stateflows.StateMachines;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception);

        bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception);

        bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        bool OnStateInitializationException(IStateActionContext context, Exception exception);

        bool OnStateFinalizationException(IStateActionContext context, Exception exception);

        bool OnStateEntryException(IStateActionContext context, Exception exception);

        bool OnStateExitException(IStateActionContext context, Exception exception);
    }
}
