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

        public Task<RequestResult<CompoundResponse>> SendCompoundAsync(Action<ICompoundRequestBuilder> builderAction, IEnumerable<EventHeader> headers = null)
        {
            var compound = new CompoundRequestBuilderRequest();
            builderAction(compound);
            return RequestAsync(compound, headers);
        }

        public Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full, IEnumerable<EventHeader> headers = null)
            => SendAsync(new Reset { Mode = resetMode }, headers);

        public Task<SendResult> FinalizeAsync(IEnumerable<EventHeader> headers = null)
            => SendAsync(new Finalize(), headers);

        public Task<RequestResult<BehaviorInfo>> GetStatusAsync(IEnumerable<EventHeader> headers = null)
            => RequestAsync(new BehaviorInfoRequest(), headers);

        public Task<IWatcher> WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        public Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(DateTime? lastNotificationsCheck = null);

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(string[] notificationNames, DateTime? lastNotificationsCheck = null);
    }
}