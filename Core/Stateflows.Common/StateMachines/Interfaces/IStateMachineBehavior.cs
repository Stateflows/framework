using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Events;

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
