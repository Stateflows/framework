using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Lock
{
    internal class InProcessLock : IStateflowsLock
    {
        private Dictionary<BehaviorId, EventWaitHandle> Events { get; } = new Dictionary<BehaviorId, EventWaitHandle>();

        public Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
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

            @event.WaitOne((int)(timeout?.TotalMilliseconds ?? -1));

            return Task.FromResult(new StateflowsLockHandle(id, new AsyncDisposableHandle(@event)) as IStateflowsLockHandle);
        }
    }
}
