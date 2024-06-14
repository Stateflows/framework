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

            return builder.AddExceptionHandler<TException>(async c =>
            {
                var handler = (c as BaseContext).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c);

                if (handler != null)
                {
                    ActivityNodeContextAccessor.Context.Value = c;
                    await handler?.HandleAsync();
                    ActivityNodeContextAccessor.Context.Value = null;
                }
            });
        }
    }
}
