using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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

        public Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full, IEnumerable<EventHeader> headers = null)
            => SendAsync(new Reset { Mode = resetMode }, headers);

        public Task<SendResult> FinalizeAsync(IEnumerable<EventHeader> headers = null)
            => SendAsync(new Finalize(), headers);
        
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

        public Task<RequestResult<BehaviorInfo>> GetStatusAsync(IEnumerable<EventHeader> headers = null)
            => RequestAsync(new BehaviorInfoRequest(), headers);

        public Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);
        
        public Task<IWatcher> RequestAndWatchStatusAsync(Action<BehaviorInfo> handler, IEnumerable<EventHeader> headers = null)
            => RequestAndWatchAsync(new BehaviorInfoRequest(), handler, headers);
        
        public Task<RequestResult<NotificationsResponse>> GetNotificationsAsync<TNotification>(TimeSpan? period = null, IEnumerable<EventHeader> headers = null)
            => RequestAsync(new NotificationsRequest()
            {
                Period = period ?? TimeSpan.FromSeconds(60),
                NotificationNames = new List<string>() { Event<TNotification>.Name }
            }, headers);
        
        public Task<RequestResult<NotificationsResponse>> GetNotificationsAsync(string[] notificationNames, TimeSpan? period = null, IEnumerable<EventHeader> headers = null)
            => RequestAsync(new NotificationsRequest()
            {
                Period = period ?? TimeSpan.FromSeconds(60),
                NotificationNames = notificationNames.ToList()
            }, headers);
    }
}