using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityBehavior : IBehavior, IInputOutput
    {
        public new Task<RequestResult<ActivityInfo>> GetStatusAsync(IEnumerable<EventHeader> headers = null)
            => RequestAsync(new ActivityInfoRequest(), headers);

        public Task<IWatcher> WatchStatusAsync(Action<ActivityInfo> handler)
            => WatchAsync(handler);
        
        [Obsolete("Use retained notification Events instead")]
        public Task<IWatcher> RequestAndWatchStatusAsync(Action<ActivityInfo> handler, IEnumerable<EventHeader> headers = null)
            => RequestAndWatchAsync(new ActivityInfoRequest(), handler, headers);
        
        public Task<RequestResult<TokensOutput>> GetOutputAsync()
            => RequestAsync(new TokensOutputRequest());
        
        public Task<RequestResult<TokensOutput<T>>> GetOutputAsync<T>()
            => RequestAsync(new TokensOutputRequest<T>());
    }
}
