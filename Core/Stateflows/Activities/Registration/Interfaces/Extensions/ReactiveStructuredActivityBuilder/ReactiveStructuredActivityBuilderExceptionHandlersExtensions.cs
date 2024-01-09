using System;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderExceptionHandlersExtensions
    {
        public static IReactiveStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IReactiveStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandler<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IReactiveStructuredActivityBuilder;
    }
}
