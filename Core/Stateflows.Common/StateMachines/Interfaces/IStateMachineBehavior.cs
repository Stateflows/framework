using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        public Task<RequestResult<StateMachineInfo>> GetCurrentStateAsync(IEnumerable<EventHeader> headers = null)
            => RequestAsync(new StateMachineInfoRequest(), headers);

        public Task<IWatcher> WatchCurrentStateAsync(Action<StateMachineInfo> handler)
            => WatchAsync(handler);
        
        public Task<IWatcher> RequestAndWatchCurrentStateAsync(Action<StateMachineInfo> handler, IEnumerable<EventHeader> headers = null)
            => RequestAndWatchAsync(new StateMachineInfoRequest(), handler, headers);
    }
}
