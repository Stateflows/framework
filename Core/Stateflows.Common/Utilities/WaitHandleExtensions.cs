using System.Threading.Tasks;
using System.Threading;
using Stateflows.Activities;

namespace Stateflows.Common.Utilities
{
    public static class WaitHandleExtensions
    {
        public static Task WaitOneAsync(this WaitHandle waitHandle)
            => waitHandle.WaitOneAsync(-1);

        public static Task WaitOneAsync(this WaitHandle waitHandle, int millisecondsTimeout)
        {
            waitHandle.ThrowIfNull(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();
            var registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                delegate { tcs.TrySetResult(true); },
                null,
                millisecondsTimeout,
                true
            );

            var t = tcs.Task;

            t.ContinueWith((antecedent) => registeredWaitHandle.Unregister(null));

            return t;
        }
    }
}
