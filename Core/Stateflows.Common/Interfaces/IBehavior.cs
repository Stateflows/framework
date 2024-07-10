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

        //Task<RequestResult<InitializationResponse>> InitializeAsync(InitializationRequest initializationEvent = null)
        //    => RequestAsync(initializationEvent ?? new InitializationRequest());

        //Task<RequestResult<FinalizationResponse>> FinalizeAsync()
        //    => RequestAsync(new FinalizationRequest());

        Task<RequestResult<ResetResponse>> ResetAsync(ResetMode resetMode = ResetMode.Full)
            => RequestAsync(new ResetRequest() { Mode = resetMode });

        //async Task<SendResult> ReinitializeAsync(Event initializationEvent = null, ResetMode resetMode = ResetMode.Full)
        //{
        //    initializationEvent ??= new Initialize();
        //    var compoundResult = await SendCompoundAsync(
        //        new ResetRequest() { Mode = resetMode },
        //        initializationEvent
        //    );

        //    var result = compoundResult.Response.Results.Last();

        //    return new SendResult(initializationEvent, result.Status, result.Validation);
        //}

        Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync()
            => RequestAsync(new BehaviorStatusRequest());

        Task WatchStatusAsync(Action<BehaviorStatusNotification> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorStatusNotification>();
    }
}