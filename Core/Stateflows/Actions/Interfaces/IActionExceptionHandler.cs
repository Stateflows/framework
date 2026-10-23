using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>
    /// Acknowledges action-body failures without recovering execution. All handlers are awaited sequentially;
    /// returning true acknowledges the exception. Execution still fails and its context
    /// changes are not persisted. IDE break-on-thrown settings control debugger stops
    /// independently of acknowledgement.
    /// </summary>
    public interface IActionExceptionHandler
    {
        Task<bool> OnActionExceptionAsync(IActionDelegateContext context, Exception exception);
    }
}
