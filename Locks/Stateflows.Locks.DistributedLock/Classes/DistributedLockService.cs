﻿using System;
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

        public async Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
        {
            var distributedLock = await DistributedLockFactory(id.ToString());
            var handle = distributedLock.Acquire(timeout);

            return new StateflowsLockHandle(id, handle);
        }

        public async Task<IStateflowsLockHandle> AquireValuesLockAsync(BehaviorId id, TimeSpan? timeout = null)
        {
            var distributedLock = await DistributedLockFactory($"{id.ToString()}.values");
            var handle = distributedLock.Acquire(timeout);

            return new StateflowsLockHandle(id, handle);
        }
    }
}
