using System;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsLockHandle : IAsyncDisposable
    {
        public BehaviorId BehaviorId { get; }
    }
}
