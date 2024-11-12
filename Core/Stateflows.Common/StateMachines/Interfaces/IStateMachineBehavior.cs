using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        Task<RequestResult<StateMachineInfo>> GetCurrentStateAsync()
            => RequestAsync(new StateMachineInfoRequest());

        async Task<IWatcher> WatchCurrentStateAsync(Action<StateMachineInfo> handler, bool immediateRequest = true)
        {
            var watcher = await WatchAsync(handler);

            if (immediateRequest)
            {
                var result = await GetCurrentStateAsync();
                if (result.Status == EventStatus.Consumed)
                {
                    _ = Task.Run(() => handler(result.Response));
                }
            }

            return watcher;
        }
    }
}
