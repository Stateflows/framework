using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>Observes action initialization and finalization operations.</summary>
    /// <remarks>
    /// Each observer runs around the operation and remaining observers. Global
    /// observers wrap local observers, with each group applied in registration order.
    /// Observers must invoke and await their continuation exactly once. They cannot
    /// replace operation results or suppress continuation failures; use
    /// <see langword="try"/>/<see langword="finally"/> for cleanup that must also run
    /// when the continuation fails. Missing, repeated, or unfinished continuation
    /// calls fail execution, and an exception thrown by an observer also propagates
    /// as an operation failure.
    /// </remarks>
    public interface IActionObserver
    {
        /// <summary>
        /// Observes action initialization. The callback begins before the remaining
        /// observers and initialization delegate, and completes after the continuation
        /// has finished.
        /// </summary>
        /// <param name="actionContext">The delegate context for the action being initialized.</param>
        /// <param name="next">The continuation for remaining observers and the initialization delegate; it must be invoked and awaited exactly once.</param>
        /// <returns>A task completed when observation and the initialization operation have completed.</returns>
        Task OnActionInitializeAsync(IActionDelegateContext actionContext, Func<Task> next);

        /// <summary>
        /// Observes action finalization. The callback begins before the remaining
        /// observers and finalization delegate, and completes after the continuation
        /// has finished.
        /// </summary>
        /// <param name="actionContext">The delegate context for the action being finalized.</param>
        /// <param name="next">The continuation for remaining observers and the finalization delegate; it must be invoked and awaited exactly once.</param>
        /// <returns>A task completed when observation and the finalization operation have completed.</returns>
        Task OnActionFinalizeAsync(IActionDelegateContext actionContext, Func<Task> next);
    }
}
