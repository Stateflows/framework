using System;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class TypedActionBuilderExceptionHandlersExtensions
    {
        public static ITypedActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this ITypedActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as ITypedActionBuilder;
    }
}
