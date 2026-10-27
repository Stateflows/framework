using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>
    /// Observes action lifecycle operations. Global observers wrap local observers,
    /// in registration order within each group.
    /// </summary>
    /// <remarks>
    /// Call and await next exactly once. Observers must not change state or suppress
    /// continuation failures. Use try/finally for cleanup that must run on failure.
    /// Missing, repeated, or unfinished continuation calls fail execution.
    /// </remarks>
    public interface IActionObserver
    {
        /// <summary>Observes action initialization.</summary>
        /// <param name="context">The action delegate context.</param>
        /// <param name="next">The continuation for the remaining observers and initialization operation.</param>
        Task OnActionInitializeAsync(IActionDelegateContext context, Func<Task> next);

        /// <summary>Observes action finalization.</summary>
        /// <param name="context">The action delegate context.</param>
        /// <param name="next">The continuation for the remaining observers and finalization operation.</param>
        Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next);
    }
}
