using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Base observer that forwards every operation to its continuation.</summary>
    /// <remarks>Override only the operations of interest and preserve the exactly-once, awaited continuation contract.
    /// See <see href="https://github.com/Stateflows/framework/wiki/4.5.-Observability">observability</see>.</remarks>
    public abstract class StateMachineObserver : IStateMachineObserver
    {
        /// <inheritdoc/>
        public virtual Task OnActionAsync(IStateMachineActionObservationContext actionContext, Func<Task> next) => next();
        /// <inheritdoc/>
        public virtual Task OnPredicateAsync(IStateMachinePredicateObservationContext predicateContext, Func<Task<bool>> next) => next();
        /// <inheritdoc/>
        public virtual Task OnHostedBehaviorInitializeAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next) => next();
        /// <inheritdoc/>
        public virtual Task OnHostedBehaviorFinalizeAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next) => next();
        /// <inheritdoc/>
        public virtual Task OnHostedBehaviorEventForwardAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next) => next();

        /// <inheritdoc/>
        public virtual Task OnDeferralGuardAsync(IDeferralObservationContext deferralContext, Func<Task<bool>> next) => next();

        /// <inheritdoc/>
        public virtual Task OnEventDeferredAsync(IDeferralObservationContext deferralContext, Func<Task> next) => next();

        /// <inheritdoc/>
        public virtual Task OnStateMachineInitializeAsync(IStateMachineInitializationContext initializationContext, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateMachineFinalizeAsync(IStateMachineActionContext finalizationContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateInitializeAsync(IStateActionContext stateContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateFinalizeAsync(IStateActionContext stateContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateEntryAsync(IStateActionContext stateContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateExitAsync(IStateActionContext stateContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Func<Task> next)
            => next();
    }
}