using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Classes
{
    public class InProcessLock : IStateflowsLock
    {
        public Dictionary<int, EventWaitHandle> Events { get; } = new Dictionary<int, EventWaitHandle>();

        public async Task LockAsync(BehaviorId id)
        {
            EventWaitHandle @event = null;

            lock (Events)
            {
                if (!Events.TryGetValue(id.GetHashCode(), out @event))
                {
                    @event = new EventWaitHandle(true, EventResetMode.AutoReset);
                    Events.Add(id.GetHashCode(), @event);
                }
            }

            await @event.WaitOneAsync();
            @event.Reset();
        }

        public Task UnlockAsync(BehaviorId id)
        {
            EventWaitHandle @event = null;

            lock (Events)
            {
                if (!Events.TryGetValue(id.GetHashCode(), out @event))
                {
                    @event = new EventWaitHandle(false, EventResetMode.AutoReset);
                    Events.Add(id.GetHashCode(), @event);
                }
            }

            @event.Set();

            return Task.CompletedTask;
        }
    }
}
