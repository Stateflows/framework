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
        private Dictionary<string, EventWaitHandle> Events { get; } = new Dictionary<string, EventWaitHandle>();

        public Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
            => AquireLockAsync(id, string.Empty, timeout);
        
        public Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, string scope, TimeSpan? timeout = null)
        {
            EventWaitHandle @event = null;
            var idString = scope == string.Empty ? id.ToString() : $"{id.ToString()}.{scope}";
            
            lock (Events)
            {
                if (!Events.TryGetValue(idString, out @event))
                {
                    @event = new EventWaitHandle(true, EventResetMode.AutoReset);
                    Events.Add(idString, @event);
                }
            }

            @event.WaitOne((int)(timeout?.TotalMilliseconds ?? -1));

            return Task.FromResult(new StateflowsLockHandle(id, scope, new AsyncDisposableHandle(@event)) as IStateflowsLockHandle);
        }
    }
}
