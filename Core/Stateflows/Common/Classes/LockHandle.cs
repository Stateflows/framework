using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class LockHandle : IStateflowsLockHandle
    {
        public LockHandle(BehaviorId behaviorId, IAsyncDisposable handle)
        {
            BehaviorId = behaviorId;
            Handle = handle;
        }

        public BehaviorId BehaviorId { get; }

        private readonly IAsyncDisposable Handle;

        public ValueTask DisposeAsync()
            => Handle.DisposeAsync();
    }
}