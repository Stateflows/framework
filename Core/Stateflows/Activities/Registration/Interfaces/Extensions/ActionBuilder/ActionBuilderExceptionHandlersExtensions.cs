﻿using System;
using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActionBuilderExceptionHandlersExtensions
    {
        [DebuggerHidden]
        public static IActionBuilder AddExceptionHandler<TException, TExceptionHandler>(this IActionBuilder builder)
            where TException : Exception
            where TExceptionHandler : class, IExceptionHandlerNode<TException>
        {
            (builder as IInternal).Services.AddServiceType<TExceptionHandler>();

            return builder.AddExceptionHandler<TException>(async c =>
            {
                var handler = (c as BaseContext).NodeScope.GetExceptionHandler<TException, TExceptionHandler>(c);

                if (handler != null)
                {
                    ActivityNodeContextAccessor.Context.Value = c;
                    OutputTokens.TokensHolder.Value = (c as ExceptionHandlerContext<TException>).OutputTokens;
                    await handler?.HandleAsync(c.Exception, c.CancellationToken);
                    OutputTokens.TokensHolder.Value = null;
                    ActivityNodeContextAccessor.Context.Value = null;
                }
            });
        }
    }
}
