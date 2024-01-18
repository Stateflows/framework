using System;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderExceptionHandlersExtensions
    {
        public static IReactiveStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IReactiveStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IReactiveStructuredActivityBuilder;
    }
}
