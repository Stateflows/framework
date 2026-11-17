using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Base exception handler that leaves every failure unacknowledged unless an operation is overridden.</summary>
    /// <remarks>Returning true from an override acknowledges a failure but does not recover execution.
    /// See <see href="https://github.com/Stateflows/framework/wiki/4.5.-Observability">observability</see>.</remarks>
    public abstract class StateMachineExceptionHandler : IStateMachineExceptionHandler
    {
        /// <inheritdoc/>
        public virtual Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext initializationContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext finalizationContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnDeferralGuardExceptionAsync(IDeferralObservationContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateInitializationExceptionAsync(IStateActionContext stateContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext stateContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateEntryExceptionAsync(IStateActionContext stateContext, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateExitExceptionAsync(IStateActionContext stateContext, Exception exception)
            => Task.FromResult(false);
    }
}