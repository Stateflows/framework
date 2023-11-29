using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Utilities
{
    public static class WaitHandleExtensions
    {
        public static Task WaitOneAsync(this WaitHandle waitHandle, int millisecondsTimeout = -1)
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
