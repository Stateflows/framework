using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine;

internal static class AsyncAroundPipeline
{
    public static Task RunAsync<TInterceptor>(
        IReadOnlyList<TInterceptor> interceptors,
        Func<Task> next,
        Func<TInterceptor, Func<Task>, Task> intercept)
    {
        var pipeline = next;
        for (var i = interceptors.Count - 1; i >= 0; i--)
        {
            var interceptor = interceptors[i];
            var inner = pipeline;
            pipeline = async () =>
            {
                var continuation = new Continuation(inner, interceptor!);
                try
                {
                    await intercept(interceptor, continuation.InvokeAsync);
                }
                catch
                {
                    await continuation.DrainAsync();
                    throw;
                }

                await continuation.VerifyAsync();
            };
        }

        return pipeline();
    }

    private sealed class Continuation(Func<Task> next, object owner)
    {
        private int invocationCount;
        private Task? inFlight;
        private ExceptionDispatchInfo? failure;

        public Task InvokeAsync()
        {
            if (Interlocked.Increment(ref invocationCount) != 1)
                throw new InvalidOperationException($"{Description} invoked its continuation more than once.");

            var task = RunAsync();
            Interlocked.Exchange(ref inFlight, task);
            return task;
        }

        private async Task RunAsync()
        {
            try
            {
                await next();
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
                throw;
            }
        }

        public async Task DrainAsync()
        {
            var task = Interlocked.CompareExchange(ref inFlight, null, null);
            if (task == null || task.IsCompleted) return;

            try
            {
                await task;
            }
            catch
            {
            }
        }

        public async Task VerifyAsync()
        {
            if (Interlocked.CompareExchange(ref invocationCount, 0, 0) > 1)
            {
                await DrainAsync();
                throw new InvalidOperationException($"{Description} invoked its continuation more than once.");
            }

            if (Interlocked.CompareExchange(ref invocationCount, 0, 0) == 0) return;

            var task = Interlocked.CompareExchange(ref inFlight, null, null);
            if (task == null || !task.IsCompleted)
            {
                await DrainAsync();
                throw new InvalidOperationException($"{Description} completed before its continuation finished; the continuation must be awaited.");
            }

            failure?.Throw();
        }

        private string Description => $"Interceptor '{owner.GetType().FullName}'";
    }
}
