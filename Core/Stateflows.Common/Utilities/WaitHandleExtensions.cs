using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Utilities
{
    public static class WaitHandleExtensions
    {
        private static AsyncLocal<TaskCompletionSource<bool>> TaskCompletionSource =  new AsyncLocal<TaskCompletionSource<bool>>();
        
        [DebuggerHidden]
        private static void Callback(object state, bool timedOut)
        {
            TaskCompletionSource.Value.TrySetResult(true);
        }
        
        [DebuggerHidden]
        public static Task WaitOneAsync(this WaitHandle waitHandle, int millisecondsTimeout = -1)
        {
            waitHandle.ThrowIfNull(nameof(waitHandle));

            TaskCompletionSource.Value = new TaskCompletionSource<bool>();
            var registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                Callback,
                null,
                millisecondsTimeout,
                true
            );

            var t = TaskCompletionSource.Value.Task;

            t.ContinueWith((antecedent) => registeredWaitHandle.Unregister(null));

            TaskCompletionSource.Value = null;

            return t;
        }
    }
}
