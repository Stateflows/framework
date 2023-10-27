using System;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderExceptionHandlersExtensions
    {
        public static IStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandler<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IStructuredActivityBuilder;
    }
}
