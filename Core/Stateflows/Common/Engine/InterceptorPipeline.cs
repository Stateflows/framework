using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine
{
    internal static class InterceptorPipeline
    {
        public static Task<EventStatus> RunAsync<TInterceptor>(
            IReadOnlyList<TInterceptor> interceptors,
            Func<Task<EventStatus>> next,
            Func<TInterceptor, Func<Task<EventStatus>>, Task<EventStatus>> intercept)
        {
            var pipeline = next;
            for (var i = interceptors.Count - 1; i >= 0; i--)
            {
                var interceptor = interceptors[i];
                var inner = pipeline;
                pipeline = async () =>
                {
                    var continuation = new ExecutionContinuation<EventStatus>(inner, interceptor!, "Interceptor");
                    EventStatus status;
                    try
                    {
                        status = await intercept(interceptor, continuation.InvokeAsync);
                    }
                    catch
                    {
                        await continuation.DrainAsync();
                        throw;
                    }

                    await continuation.VerifyAsync(required: false);
                    return status;
                };
            }

            return pipeline();
        }
    }
}
