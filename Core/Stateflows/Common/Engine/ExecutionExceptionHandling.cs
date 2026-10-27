using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Context;

namespace Stateflows.Common.Engine
{
    internal static class ExecutionExceptionHandling
    {
        public static async Task<bool> HandleAsync<THandler>(
            StateflowsContext context,
            IEnumerable<THandler> handlers,
            Func<THandler, Task<bool>> handle,
            string method,
            ILogger logger)
        {
            var acknowledged = false;
            foreach (var handler in handlers)
            {
                try
                {
                    acknowledged |= await handle(handler);
                }
                catch (Exception exception)
                {
                    context.ExecutionExceptions.Enqueue(exception);
                    logger.LogError(exception, "Exception handler {HandlerType}.{Method} failed for {BehaviorId}.",
                        handler?.GetType().FullName, method, context.Id);
                }
            }

            return acknowledged;
        }
    }
}
