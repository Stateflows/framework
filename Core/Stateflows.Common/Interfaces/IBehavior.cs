using System;
using System.Collections.Generic;
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

        Task<RequestResult<CompoundResponse>> SendCompoundAsync(params Event[] events)
            => RequestAsync(new CompoundRequest() { Events = events });

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request, IEnumerable<EventHeader> headers = null);

        Task<SendResult> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => SendAsync(new Reset() { Mode = resetMode });

        Task<RequestResult<FinalizationResponse>> FinalizeAsync()
            => RequestAsync(new FinalizationRequest());

        // Task<RequestResult<ResetResponse>> ResetAsync(ResetMode resetMode = ResetMode.Full)
        //     => RequestAsync(new ResetRequest() { Mode = resetMode });

        Task<RequestResult<BehaviorInfo>> GetStatusAsync()
            => RequestAsync(new BehaviorInfoRequest());

        Task WatchStatusAsync(Action<BehaviorInfo> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorInfo>();
    }
}