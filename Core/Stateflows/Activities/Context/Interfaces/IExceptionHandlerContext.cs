using System;

namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context supplied to an activity exception handler.</summary>
    /// <typeparam name="TException">The exception type handled by this callback.</typeparam>
    /// <remarks>Tokens explicitly output through this context follow the exception-handler recovery path.</remarks>
    public interface IExceptionHandlerContext<out TException> : IExceptionContext, IActivityNodeContext, IOutput
        where TException : Exception
    {
        /// <summary>The exception being handled.</summary>
        TException Exception { get; }
    }
}
