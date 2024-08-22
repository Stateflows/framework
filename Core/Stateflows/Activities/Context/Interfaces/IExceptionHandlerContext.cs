using System;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IExceptionHandlerContext<out TException> : IExceptionContext, IActivityNodeContext, IActionOutput
        where TException : Exception
    {
        TException Exception { get; }
    }
}
