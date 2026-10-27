using System;
using System.Threading.Tasks;
using Stateflows.Common.Classes;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Acquires execution locks for behavior instances.</summary>
    public interface IStateflowsLock
    {
        /// <summary>Acquires a lock for a behavior.</summary>
        /// <param name="id">The behavior whose execution is locked.</param>
        /// <param name="timeout">An optional maximum time to wait for the lock.</param>
        /// <returns>A handle that releases the lock when disposed.</returns>
        Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null);
        
        /// <summary>Acquires a lock for a behavior within a named scope.</summary>
        /// <param name="id">The behavior whose execution is locked.</param>
        /// <param name="scope">The lock scope, allowing independent locks for the same behavior.</param>
        /// <param name="timeout">An optional maximum time to wait for the lock.</param>
        /// <returns>A handle that releases the lock when disposed.</returns>
        Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, string scope, TimeSpan? timeout = null);
        
        /// <summary>Creates a no-op lock handle for a behavior.</summary>
        /// <param name="id">The behavior associated with the handle.</param>
        /// <returns>A handle whose disposal does not release a physical lock.</returns>
        public Task<IStateflowsLockHandle> AquireNoLockAsync(BehaviorId id)
            => Task.FromResult(new StateflowsNoLockHandle(id) as IStateflowsLockHandle);
    }
}
