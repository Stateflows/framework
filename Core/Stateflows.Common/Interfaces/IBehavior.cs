using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IBehavior : IWatches
    {
        BehaviorId Id { get; }

        Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new();

        Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        Task<RequestResult<InitializationResponse>> InitializeAsync(InitializationRequest initializationRequest = null)
            => RequestAsync(initializationRequest ?? new InitializationRequest());

        Task<RequestResult<FinalizationResponse>> FinalizeAsync()
            => RequestAsync(new FinalizationRequest());
    }
}