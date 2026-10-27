using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>
    /// Observes state-machine operations. Global observers wrap local observers,
    /// in registration order within each group.
    /// </summary>
    /// <remarks>
    /// Call and await next exactly once. Results are exposed for inspection, not
    /// replacement. Observers must not change state or suppress continuation failures.
    /// Use try/finally for cleanup that must run on failure. Missing, repeated, or
    /// unfinished continuation calls fail execution.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.
    /// </remarks>
    public interface IStateMachineObserver
    {
        /// <summary>Observes an initialization attempt, including its acceptance result.</summary>
        /// <param name="context">The initializing machine's execution context.</param>
        /// <param name="implicitInitialization">Whether processing an ordinary event caused initialization rather than an explicit initialization request.</param>
        /// <param name="next">The continuation; call and await exactly once. Its result indicates whether initialization was accepted.</param>
        /// <returns>A task completing after observation and the initialization attempt.</returns>
        Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next);

        /// <summary>Observes machine finalization after its final configuration is reached or finalization is requested.</summary>
        /// <param name="context">The machine execution context.</param>
        /// <param name="next">The finalization continuation; call and await exactly once.</param>
        /// <returns>A task completing after finalization and observation.</returns>
        Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next);

        /// <summary>Observes initialization of a composite or orthogonal state's nested configuration.</summary>
        /// <param name="context">The initializing state's action context.</param>
        /// <param name="next">The initialization continuation; call and await exactly once.</param>
        /// <returns>A task completing after the observed state operation.</returns>
        Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next);

        /// <summary>Observes finalization of a composite or orthogonal state's nested configuration.</summary>
        /// <param name="context">The finalizing state's action context.</param>
        /// <param name="next">The finalization continuation; call and await exactly once.</param>
        /// <returns>A task completing after the observed state operation.</returns>
        Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next);

        /// <summary>Observes entry into a state, including its registered entry actions.</summary>
        /// <param name="context">The entered state's action context.</param>
        /// <param name="next">The entry continuation; call and await exactly once.</param>
        /// <returns>A task completing after entry and observation.</returns>
        Task OnStateEntryAsync(IStateActionContext context, Func<Task> next);

        /// <summary>Observes exit from a state, including its registered exit actions.</summary>
        /// <param name="context">The exited state's action context.</param>
        /// <param name="next">The exit continuation; call and await exactly once.</param>
        /// <returns>A task completing after exit and observation.</returns>
        Task OnStateExitAsync(IStateActionContext context, Func<Task> next);

        /// <summary>Observes transition eligibility without replacing its guard result.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="context">The candidate transition and triggering event.</param>
        /// <param name="next">The guard continuation; call and await exactly once to inspect its result.</param>
        /// <returns>A task completing after guard evaluation and observation.</returns>
        Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next);

        /// <summary>Observes the effect executed when a transition is taken.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="context">The selected transition and triggering event.</param>
        /// <param name="next">The effect continuation; call and await exactly once.</param>
        /// <returns>A task completing after the effect and observation.</returns>
        Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next);
    }
}
