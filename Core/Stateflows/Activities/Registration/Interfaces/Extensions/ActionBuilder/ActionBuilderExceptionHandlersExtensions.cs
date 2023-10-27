using System;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActionBuilderExceptionHandlersExtensions
    {
        public static IActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this IActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandler<TException>
        {
            (builder as IInternal).Services.RegisterExceptionHandler<TException, TExceptionHandler>();

            return builder.AddExceptionHandler<TException>(c => (c as BaseContext).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c).HandleAsync());
        }
    }
}
