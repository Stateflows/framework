using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class StateflowsLockHandle : IStateflowsLockHandle
    {
        public StateflowsLockHandle(BehaviorId behaviorId, IAsyncDisposable handle)
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