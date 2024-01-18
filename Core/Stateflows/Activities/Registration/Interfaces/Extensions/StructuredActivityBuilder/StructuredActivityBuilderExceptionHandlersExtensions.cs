using System;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderExceptionHandlersExtensions
    {
        public static IStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IStructuredActivityBuilder;
    }
}
