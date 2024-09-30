using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        Task<RequestResult<CurrentState>> GetCurrentStateAsync()
            => RequestAsync(new CurrentStateRequest());

        async Task<IWatcher> WatchCurrentStateAsync(Action<CurrentState> handler, bool immediateRequest = true)
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
