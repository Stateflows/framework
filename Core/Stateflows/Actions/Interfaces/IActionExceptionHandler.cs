using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionExceptionHandler
    {
        bool OnActionException(IActionDelegateContext context, Exception exception);
    }
}
