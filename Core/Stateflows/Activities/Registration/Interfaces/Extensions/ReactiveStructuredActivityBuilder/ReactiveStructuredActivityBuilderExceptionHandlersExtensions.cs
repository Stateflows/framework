using System;
using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderExceptionHandlersExtensions
    {
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IReactiveStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IReactiveStructuredActivityBuilder;
    }
}
