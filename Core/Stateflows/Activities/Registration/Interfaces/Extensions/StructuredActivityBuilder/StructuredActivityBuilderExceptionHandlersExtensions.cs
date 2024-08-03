using System;
using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderExceptionHandlersExtensions
    {
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddExceptionHandler<TException, TExceptionHandler>(this IStructuredActivityBuilder builder)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
            => (builder as IActionBuilder).AddExceptionHandler<TException, TExceptionHandler>() as IStructuredActivityBuilder;
    }
}
