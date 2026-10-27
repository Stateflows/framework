using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine
{
    internal static class ObserverPipeline
    {
        public static Task RunAsync<TObserver>(
            IReadOnlyList<TObserver> observers, Func<Task> next,
            Func<TObserver, Func<Task>, Task> observe)
            => RunAsync(observers,
                async () => { await next(); return true; },
                (observer, inner) => observe(observer, () => inner()));

        public static Task<TResult> RunAsync<TObserver, TResult>(
            IReadOnlyList<TObserver> observers, Func<Task<TResult>> next,
            Func<TObserver, Func<Task<TResult>>, Task> observe)
        {
            var pipeline = next;
            for (var i = observers.Count - 1; i >= 0; i--)
            {
                var observer = observers[i];
                var inner = pipeline;
                pipeline = async () =>
                {
                    var continuation = new ExecutionContinuation<TResult>(inner, observer!, "Observer");
                    try
                    {
                        await observe(observer, continuation.InvokeAsync);
                    }
                    catch
                    {
                        await continuation.DrainAsync();
                        throw;
                    }

                    await continuation.VerifyAsync(required: true);
                    continuation.RethrowSwallowedFailure();
                    return continuation.Result;
                };
            }

            return pipeline();
        }
    }
}
