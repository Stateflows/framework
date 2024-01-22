using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Classes;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Lock
{
    internal class InProcessLock : IStateflowsLock
    {
        public Dictionary<BehaviorId, EventWaitHandle> Events { get; } = new Dictionary<BehaviorId, EventWaitHandle>();

        public async Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
        {
            EventWaitHandle @event = null;

            lock (Events)
            {
                if (!Events.TryGetValue(id, out @event))
                {
                    @event = new EventWaitHandle(true, EventResetMode.AutoReset);
                    Events.Add(id, @event);
                }
            }

            await @event.WaitOneAsync((int)(timeout?.TotalMilliseconds ?? -1));

            return new LockHandle(id, new AsyncDisposableHandle(@event));
        }
    }
}
