using Stateflows.Common;
using Stateflows.Common.Extensions;

namespace Stateflows.Transport.Http.Client
{
    internal class Behavior : IBehavior
    {
        private readonly StateflowsApiClient _apiClient;

        public BehaviorId Id { get; }

        public Behavior(StateflowsApiClient apiClient, BehaviorId id)
        {
            _apiClient = apiClient;
            Id = id;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => await _apiClient.SendAsync(Id, @event);

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var result = await SendAsync(request as Event);
            return new RequestResult<TResponse>(request, result.Status, result.Validation);
        }
    }
}
