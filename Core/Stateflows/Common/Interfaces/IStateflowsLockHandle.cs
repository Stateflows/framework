using System;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Represents a held behavior lock and releases it asynchronously when disposed.</summary>
    public interface IStateflowsLockHandle : IAsyncDisposable
    {
        /// <summary>Gets the behavior associated with this lock.</summary>
        public BehaviorId BehaviorId { get; }
    }
}
