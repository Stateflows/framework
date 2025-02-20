using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        public Task<RequestResult<StateMachineInfo>> GetCurrentStateAsync()
            => RequestAsync(new StateMachineInfoRequest());

        public Task<IWatcher> WatchCurrentStateAsync(Action<StateMachineInfo> handler)
            => WatchAsync(handler);
        
        public Task<IWatcher> RequestAndWatchCurrentStateAsync(Action<StateMachineInfo> handler)
            => RequestAndWatchAsync(new StateMachineInfoRequest(), handler);
    }
}
