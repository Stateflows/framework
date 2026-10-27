using System;
using System.Diagnostics;
using Stateflows.Activities.Context.Classes;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds node-scoped exception handlers to an activity element.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>These graph handlers are distinct from global observability exception handlers: they can handle an
    /// exception and emit recovery tokens.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
    public interface IExceptionHandlerBase<out TReturn>
    {
        /// <summary>Registers a node- or scope-level handler for exceptions raised during Activity graph execution.
        /// </summary>
        /// <typeparam name="TException">Exception type matched by this Activity handler; it must derive from
        /// <see cref="Exception" />.</typeparam>
        /// <param name="exceptionHandler">Callback handling the exception in the context of the node or enclosing
        /// structured scope.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
        TReturn AddExceptionHandler<TException>(ExceptionHandlerDelegateAsync<TException> exceptionHandler)
            where TException : Exception;
        
        /// <summary>Registers a node- or scope-level handler for exceptions raised during Activity graph execution.
        /// </summary>
        /// <typeparam name="TException">Exception type matched by this Activity handler; it must derive from
        /// <see cref="Exception" />.</typeparam>
        /// <typeparam name="TExceptionHandler">Typed Activity exception handler implementing
        /// <see cref="IActivityExceptionHandler" />.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-exception-handling" />
        [DebuggerHidden]
        public TReturn AddExceptionHandler<TException, TExceptionHandler>()
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
            => AddExceptionHandler<TException>(async c =>
            {
                var handler = await ((BaseContext)c).NodeScope.GetExceptionHandlerAsync<TException, TExceptionHandler>(c);

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
