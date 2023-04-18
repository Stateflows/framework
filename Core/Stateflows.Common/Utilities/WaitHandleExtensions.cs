using System;
using System.Threading.Tasks;
using System.Threading;

namespace Stateflows.Common.Utilities
{
    public static class WaitHandleExtensions
    {
        public static Task WaitOneAsync(this WaitHandle waitHandle)
            => waitHandle.WaitOneAsync(-1);

        public static Task WaitOneAsync(this WaitHandle waitHandle, int millisecondsTimeout)
        {
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
                delegate { tcs.TrySetResult(true); }, null, millisecondsTimeout, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }
    }
}
