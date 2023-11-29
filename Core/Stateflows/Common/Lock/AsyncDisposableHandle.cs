using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Lock
{
    internal class AsyncDisposableHandle : IAsyncDisposable
    {
        private readonly EventWaitHandle WaitHandle;

        public AsyncDisposableHandle(EventWaitHandle waitHandle)
        {
            WaitHandle = waitHandle;
        }

        public ValueTask DisposeAsync()
        {
            WaitHandle.Set();

            return new ValueTask();
        }
    }
}