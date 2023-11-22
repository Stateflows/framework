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

        private Dictionary<string, IDistributedSynchronizationHandle> Handles { get; } = new Dictionary<string, IDistributedSynchronizationHandle>();

        public DistributedLockService(Func<string, Task<IDistributedLock>> distributedLockFactory)
        {
             DistributedLockFactory = distributedLockFactory;
        }

        public async Task LockAsync(BehaviorId id)
        {
            var handle = await (await DistributedLockFactory(id.ToString())).AcquireAsync();

            lock (Handles)
            {
                if (Handles.ContainsKey(id.ToString()))
                {
                    Handles.Remove(id.ToString());
                }

                Handles.Add(id.ToString(), handle);
            }
        }

        public async Task UnlockAsync(BehaviorId id)
        {
            IDistributedSynchronizationHandle handle;

            lock (Handles)
            {
                if (Handles.TryGetValue(id.ToString(), out handle))
                {
                    Handles.Remove(id.ToString());
                }
            }

            if (handle != null)
            {
                await handle.DisposeAsync();
            }
        }
    }
}
