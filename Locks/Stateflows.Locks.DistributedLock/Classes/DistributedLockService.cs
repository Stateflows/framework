using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Medallion.Threading;
using Stateflows.Common.Interfaces;

namespace Stateflows.Locks.DistributedLock.Classes
{
    internal class DistributedLockService : IStateflowsLock
    {
        private Func<string, Task<IDistributedLock>> DistributedLockFactory { get; }

        private Dictionary<string, IDistributedLock> DistributedLocks { get; } = new Dictionary<string, IDistributedLock>();

        private Dictionary<string, IDistributedSynchronizationHandle> Handles { get; } = new Dictionary<string, IDistributedSynchronizationHandle>();

        public DistributedLockService(Func<string, Task<IDistributedLock>> distributedLockFactory)
        {
             DistributedLockFactory = distributedLockFactory;
        }

        private async Task<IDistributedLock> GetDistributedLockAsync(BehaviorId id)
        {
            if (!DistributedLocks.TryGetValue(id.ToString(), out var distributedLock))
            {
                distributedLock = await DistributedLockFactory(id.ToString());
                DistributedLocks.Add(id.ToString(), distributedLock);
            }

            return distributedLock;
        }

        public async Task Lock(BehaviorId id)
            => Handles.Add(id.ToString(), await (await GetDistributedLockAsync(id)).AcquireAsync());

        public async Task Unlock(BehaviorId id)
        {
            if (Handles.TryGetValue(id.ToString(), out var handle))
            {
                await handle.DisposeAsync();

                Handles.Remove(id.ToString());
                DistributedLocks.Remove(id.ToString());
            }
        }
    }
}
