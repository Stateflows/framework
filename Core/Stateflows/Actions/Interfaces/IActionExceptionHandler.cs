using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionExceptionHandler
    {
        Task<bool> OnActionExceptionAsync(IActionDelegateContext context, Exception exception);
    }
}
