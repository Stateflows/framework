using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Data;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IBehavior : IWatches
    {
        BehaviorId Id { get; }

        Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new();

        Task<RequestResult<CompoundResponse>> SendCompoundAsync(params Event[] events)
            => RequestAsync(new CompoundRequest() { Events = events });

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        Task<RequestResult<InitializationResponse>> InitializeAsync(InitializationRequest initializationRequest = null)
            => RequestAsync(initializationRequest ?? new InitializationRequest());

        Task<RequestResult<FinalizationResponse>> FinalizeAsync()
            => RequestAsync(new FinalizationRequest());
        Task<RequestResult<ResetResponse>> ResetAsync(bool keepVersion = false)
            => RequestAsync(new ResetRequest() { KeepVersion = keepVersion });

        async Task<RequestResult<InitializationResponse>> ReinitializeAsync(InitializationRequest initializationRequest = null, bool keepVersion = true)
        {
            initializationRequest ??= new InitializationRequest();
            var compoundResult = await SendCompoundAsync(
                new ResetRequest() { KeepVersion = keepVersion },
                initializationRequest
            );

            var result = compoundResult.Response.Results.Last();

            return new RequestResult<InitializationResponse>(initializationRequest, result.Status, result.Validation);
        }

        Task<RequestResult<InitializationResponse>> ReinitializeAsync<TInitializationPayload>(TInitializationPayload payload, bool keepVersion = true)
            => ReinitializeAsync(payload.ToInitializationRequest(), keepVersion);

        Task<RequestResult<BehaviorStatusResponse>> GetStatusAsync()
            => RequestAsync(new BehaviorStatusRequest());

        Task WatchStatusAsync(Action<BehaviorStatusNotification> handler)
            => WatchAsync(handler);

        Task UnwatchStatusAsync()
            => UnwatchAsync<BehaviorStatusNotification>();
    }
}