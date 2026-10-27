using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Base observer that forwards every operation to its continuation.</summary>
    /// <remarks>Override only the operations of interest and preserve the exactly-once, awaited continuation contract.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public abstract class StateMachineObserver : IStateMachineObserver
    {
        /// <inheritdoc/>
        public virtual Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
            => next();
    }
}