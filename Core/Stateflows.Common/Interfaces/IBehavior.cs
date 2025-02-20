using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>
    /// Behavior handle
    /// </summary>
    public interface IBehavior : IWatches, IDisposable
    {
        BehaviorId Id { get; }

        Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null);
        
        Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IEnumerable<EventHeader> headers = null);

        public Task<RequestResult<CompoundResponse>> SendCompoundAsync(Action<ICompound> builderAction, IEnumerable<EventHeader> headers = null)
        {
            var compound = new CompoundRequest();
            builderAction(compound);
            return RequestAsync(compound, headers);
        }

        public Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => SendAsync(new Reset { Mode = resetMode });

        public Task<SendResult> FinalizeAsync()
            => SendAsync(new Finalize());
        
        public async Task<IWatcher> RequestAndWatchAsync<TRequest, TNotification>(TRequest request, Action<TNotification> handler, IEnumerable<EventHeader> headers = null)
            where TRequest : IRequest<TNotification>
        {
            var watcher = await WatchAsync(handler);
            var result = await RequestAsync(request, headers);
            if (result.Status == EventStatus.Consumed)
            {
                _ = Task.Run(() => handler(result.Response));
            }
            return watcher;
        }

        public Task<RequestResult<BehaviorInfo>> GetStatusAsync()
            => RequestAsync(new BehaviorInfoRequest());

        public Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);
        
        public Task<IWatcher> RequestAndWatchStatusAsync(Action<BehaviorInfo> handler)
            => RequestAndWatchAsync(new BehaviorInfoRequest(), handler);
    }
}