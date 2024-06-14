using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ExceptionHandlerNode<TException> : ActivityNode
        where TException : Exception
    {
        new public IExceptionHandlerContext<TException> Context => base.Context as IExceptionHandlerContext<TException>;

        public abstract Task HandleAsync();
    }

    public abstract class ExceptionHandler : ExceptionHandlerNode<Exception>
    { }
}
