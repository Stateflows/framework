using System;
using System.Threading.Tasks;
using Stateflows.Common.Classes;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsLock
    {
        Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null);
        
        Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, string scope, TimeSpan? timeout = null);
        
        public Task<IStateflowsLockHandle> AquireNoLockAsync(BehaviorId id)
            => Task.FromResult(new StateflowsNoLockHandle(id) as IStateflowsLockHandle);
    }
}
