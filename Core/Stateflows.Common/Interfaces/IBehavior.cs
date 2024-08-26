using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [Obsolete("Use CompoundRequest with SendAsync() directly")]
        Task<RequestResult<CompoundResponse>> SendCompoundAsync(params Event[] events)
            => RequestAsync(new CompoundRequest() { Events = events.Select(@event => new EventHolder<Event>() { Payload = @event } as EventHolder).ToList() });

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request, IEnumerable<EventHeader> headers = null);

        Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => SendAsync(new Reset() { Mode = resetMode });

        Task<SendResult> FinalizeAsync()
            => SendAsync(new Finalize());

        Task<RequestResult<BehaviorInfo>> GetStatusAsync()
            => RequestAsync(new BehaviorInfoRequest());

        Task WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorInfo>();
    }
}