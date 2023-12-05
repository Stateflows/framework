using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class LockHandle : IStateflowsLockHandle
    {
        public LockHandle(BehaviorId behaviorId, IAsyncDisposable handle, Action disposeCallback = null)
        {
            BehaviorId = behaviorId;
            Handle = handle;
            DisposeCallback = disposeCallback;
        }

        public BehaviorId BehaviorId { get; }

        private readonly IAsyncDisposable Handle;

        private readonly Action DisposeCallback;

        public ValueTask DisposeAsync()
        {
            DisposeCallback?.Invoke();

            var result = Handle.DisposeAsync();

            return result;
        }
    }
}