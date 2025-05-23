using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public interface IStateMachineBehavior : IBehavior
    {
        public new Task<RequestResult<StateMachineInfo>> GetStatusAsync(IEnumerable<EventHeader> headers = null)
            => RequestAsync(new StateMachineInfoRequest(), headers);

        public Task<IWatcher> WatchStatusAsync(Action<StateMachineInfo> handler)
            => WatchAsync(handler);
        
        public Task<IWatcher> RequestAndWatchStatusAsync(Action<StateMachineInfo> handler, IEnumerable<EventHeader> headers = null)
            => RequestAndWatchAsync(new StateMachineInfoRequest(), handler, headers);
    }
}
