using System;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    /// <summary>Represents a behavior lock backed by an asynchronously disposable resource.</summary>
    public class StateflowsLockHandle : IStateflowsLockHandle
    {
        /// <summary>Creates a lock handle for a behavior and lock scope.</summary>
        /// <param name="behaviorId">The behavior associated with the lock.</param>
        /// <param name="scope">The scope used to acquire the lock.</param>
        /// <param name="handle">The underlying lock resource released when this handle is disposed.</param>
        public StateflowsLockHandle(BehaviorId behaviorId, string scope, IAsyncDisposable handle)
        {
            BehaviorId = behaviorId;
            Scope = scope;
            Handle = handle;
        }

        /// <summary>Gets the behavior associated with the lock.</summary>
        public BehaviorId BehaviorId { get; }

        /// <summary>Gets the scope used to acquire the lock.</summary>
        public string Scope { get; } = string.Empty;

        private readonly IAsyncDisposable Handle;

        /// <summary>Releases the underlying lock resource.</summary>
        /// <returns>A task-like value that completes when the resource is released.</returns>
        public ValueTask DisposeAsync()
            => Handle.DisposeAsync();
    }
}