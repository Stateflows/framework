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
        
        Task<SendResult> SendAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null);
        
        Task<RequestResult<TResponseEvent>> RequestAsync<TResponseEvent>(IRequest<TResponseEvent> request, IDictionary<string, EventHeader>? headers = null);
        
        public Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full, IDictionary<string, EventHeader> headers = null)
            => SendAsync(new Reset { Mode = resetMode }, headers);

        public Task<SendResult> FinalizeAsync(FinalizationMode finalizationMode = FinalizationMode.Immediate, IDictionary<string, EventHeader> headers = null)
            => SendAsync(new Finalize() { Mode = finalizationMode }, headers);

        public Task<RequestResult<BehaviorInfo>> GetStatusAsync(IDictionary<string, EventHeader> headers = null)
            => RequestAsync(new BehaviorInfoRequest(), headers);

        public Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        public Task<IWatcher> WatchStatusAsync(Func<BehaviorInfo, Task> asyncHandler)
            => WatchStatusAsync(handler: n => asyncHandler(n).GetAwaiter().GetResult());

        [Obsolete("Use WatchAsync instead.")]
        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null);
        
        [Obsolete("Use WatchAsync instead.")]
        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null);
    }
}