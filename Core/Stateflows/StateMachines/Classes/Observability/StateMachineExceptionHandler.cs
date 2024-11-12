using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineExceptionHandler : IStateMachineExceptionHandler
    {
        public virtual Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
                => Task.FromResult(false);

        public virtual Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);
    }
}