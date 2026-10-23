using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Stateflows.Common.Engine
{
    internal sealed class ExecutionContinuation<TResult>(Func<Task<TResult>> next, object owner, string ownerKind)
    {
        private int invocationCount;

        private Task<TResult>? inFlight;

        private bool invokedMoreThanOnce => System.Threading.Volatile.Read(ref invocationCount) > 1;

        private ExceptionDispatchInfo? failure;

        public TResult Result { get; private set; } = default!;

        private string OwnerDescription => $"{ownerKind} '{owner.GetType().FullName}'";

        public Task<TResult> InvokeAsync()
        {
            // Counted before execution so re-entrant or concurrent calls are rejected deterministically.
            if (Interlocked.Increment(ref invocationCount) != 1)
            {
                throw new InvalidOperationException($"{OwnerDescription} invoked its continuation more than once.");
            }

            var task = RunAsync();
            System.Threading.Volatile.Write(ref inFlight, task);

            return task;
        }

        private async Task<TResult> RunAsync()
        {
            try
            {
                return Result = await next();
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
                throw;
            }
        }

        public async Task DrainAsync()
        {
            var task = System.Threading.Volatile.Read(ref inFlight);
            if (task != null && !task.IsCompleted)
            {
                try
                {
                    await task;
                }
                catch
                {
                    // Failure is either already propagating through the hook or captured in 'failure'.
                }
            }
        }

        public async Task VerifyAsync(bool required)
        {
            if (invokedMoreThanOnce)
            {
                await DrainAsync();
                throw new InvalidOperationException($"{OwnerDescription} invoked its continuation more than once.");
            }

            if (System.Threading.Volatile.Read(ref invocationCount) == 0)
            {
                if (required)
                {
                    throw new InvalidOperationException($"{OwnerDescription} completed without invoking its continuation.");
                }

                return;
            }

            var task = System.Threading.Volatile.Read(ref inFlight);
            if (task == null || !task.IsCompleted)
            {
                Exception? innerException = null;
                try
                {
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (Exception exception)
                {
                    innerException = exception;
                }

                throw new InvalidOperationException(
                    $"{OwnerDescription} completed before its continuation finished; the continuation must be awaited.",
                    innerException
                );
            }
        }

        public void RethrowSwallowedFailure()
            => failure?.Throw();
    }
}
