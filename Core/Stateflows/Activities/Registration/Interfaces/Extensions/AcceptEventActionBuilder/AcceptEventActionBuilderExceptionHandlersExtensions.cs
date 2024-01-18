using System;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderExceptionHandlersExtensions
    {
        public static IAcceptEventActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this IAcceptEventActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : ExceptionHandlerNode<TException>
        {
            (builder as IInternal).Services.RegisterExceptionHandlerAction<TException, TExceptionHandler>();

            return builder.AddExceptionHandler<TException>(c => (c as BaseContext).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c).HandleAsync());
        }
    }
}
