using System;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActionBuilderExceptionHandlersExtensions
    {
        public static ITypedActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this ITypedActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandler<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as ITypedActionBuilder;
    }
}
