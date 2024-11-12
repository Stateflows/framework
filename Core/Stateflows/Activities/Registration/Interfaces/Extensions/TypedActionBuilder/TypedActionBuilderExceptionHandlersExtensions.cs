using System;
using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActionBuilderExceptionHandlersExtensions
    {
        [DebuggerHidden]
        public static ITypedActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this ITypedActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as ITypedActionBuilder;
    }
}
