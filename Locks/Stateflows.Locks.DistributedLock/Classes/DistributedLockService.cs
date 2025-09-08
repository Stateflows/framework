using System;
using System.Threading.Tasks;
using Medallion.Threading;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Locks.DistributedLock.Classes
{
    internal class DistributedLockService : IStateflowsLock
    {
        private Func<string, Task<IDistributedLock>> DistributedLockFactory { get; }

        public DistributedLockService(Func<string, Task<IDistributedLock>> distributedLockFactory)
        {
             DistributedLockFactory = distributedLockFactory;
        }

        public Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
            => AquireLockAsync(id, string.Empty, timeout);
        
        public async Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, string scope, TimeSpan? timeout = null)
        {
            var distributedLock = await DistributedLockFactory(scope == string.Empty ? id.ToString() : $"{id.ToString()}.{scope}");
            var handle = await distributedLock.AcquireAsync(timeout);

            return new StateflowsLockHandle(id, scope, handle);
        }
    }
}
