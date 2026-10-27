using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Base exception handler that leaves every failure unacknowledged unless an operation is overridden.</summary>
    /// <remarks>Returning true from an override acknowledges a failure but does not recover execution.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public abstract class StateMachineExceptionHandler : IStateMachineExceptionHandler
    {
        /// <inheritdoc/>
        public virtual Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);
    }
}