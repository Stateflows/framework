using System;
using System.Diagnostics;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IExceptionHandlerBase<out TReturn>
    {
        TReturn AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception;
        
        [DebuggerHidden]
        public TReturn AddExceptionHandler<TException, TExceptionHandler>()
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
            => AddExceptionHandler<TException>(async c =>
            {
                var handler = ((BaseContext)c).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c);

                if (handler != null)
                {
                    ActivityNodeContextAccessor.Context.Value = c;
                    OutputTokens.TokensHolder.Value = ((ExceptionHandlerContext<TException>)c).OutputTokens;
                    await handler?.HandleAsync(c.Exception, c.CancellationToken);
                    OutputTokens.TokensHolder.Value = null;
                    ActivityNodeContextAccessor.Context.Value = null;
                }
            });
    }
}
