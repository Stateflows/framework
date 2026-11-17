using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>
    /// Acknowledges action-body failures without recovering execution. All handlers are awaited sequentially;
    /// returning true acknowledges the exception. Execution still fails and its context
    /// changes are not persisted. IDE break-on-thrown settings control debugger stops
    /// independently of acknowledgement. Exceptions thrown by a handler are recorded
    /// and logged; they do not prevent other handlers from running or replace the
    /// original action failure.
    /// </summary>
    public interface IActionExceptionHandler
    {
        /// <summary>
        /// Handles notification of a failure from the action delegate. Handlers are
        /// awaited sequentially; returning <see langword="true"/> acknowledges the
        /// exception, but does not recover the action or persist its context changes.
        /// </summary>
        /// <param name="actionContext">The delegate context captured when the action failed.</param>
        /// <param name="exception">The exception thrown by the action delegate.</param>
        /// <returns>
        /// A task yielding <see langword="true"/> to acknowledge the exception or
        /// <see langword="false"/> to leave it unacknowledged; either result leaves
        /// action execution failed.
        /// </returns>
        Task<bool> OnActionExceptionAsync(IActionDelegateContext actionContext, Exception exception);
    }
}
