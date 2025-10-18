using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public class StateflowsLockHandle : IStateflowsLockHandle
    {
        public StateflowsLockHandle(BehaviorId behaviorId, string scope, IAsyncDisposable handle)
        {
            BehaviorId = behaviorId;
            Scope = scope;
            Handle = handle;
        }

        public BehaviorId BehaviorId { get; }

        public string Scope { get; } = string.Empty;

        private readonly IAsyncDisposable Handle;

        public ValueTask DisposeAsync()
            => Handle.DisposeAsync();
    }
}