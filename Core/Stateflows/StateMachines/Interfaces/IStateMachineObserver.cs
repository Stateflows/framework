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
    /// </remarks>
    public interface IStateMachineObserver
    {
        Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next);

        Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next);

        Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next);

        Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next);

        Task OnStateEntryAsync(IStateActionContext context, Func<Task> next);

        Task OnStateExitAsync(IStateActionContext context, Func<Task> next);

        Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next);

        Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next);
    }
}
