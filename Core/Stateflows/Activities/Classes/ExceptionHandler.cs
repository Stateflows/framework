using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ExceptionHandler<TException> : ActivityNode
        where TException : Exception
    {
        new public IExceptionHandlerContext<TException> Context { get; internal set; }

        public abstract Task HandleAsync();
    }

    public abstract class ExceptionHandler : ExceptionHandler<Exception>
    { }
}
