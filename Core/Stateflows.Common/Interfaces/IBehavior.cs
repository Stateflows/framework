using System;
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

        Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new();

        Task<RequestResult<CompoundResponse>> SendCompoundAsync(params Event[] events)
            => RequestAsync(new CompoundRequest() { Events = events });

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        Task<RequestResult<FinalizationResponse>> FinalizeAsync()
            => RequestAsync(new FinalizationRequest());

        Task<RequestResult<ResetResponse>> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => RequestAsync(new ResetRequest() { Mode = resetMode });

        Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync()
            => RequestAsync(new BehaviorStatusRequest());

        Task WatchStatusAsync(Action<BehaviorStatusNotification> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorStatusNotification>();
    }
}