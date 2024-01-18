using System;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderExceptionHandlersExtensions
    {
        public static IActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this IActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
        {
            (builder as IInternal).Services.RegisterExceptionHandlerAction<TException, TExceptionHandler>();

            return builder.AddExceptionHandler<TException>(c => (c as BaseContext).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c).HandleAsync());
        }
    }
}
