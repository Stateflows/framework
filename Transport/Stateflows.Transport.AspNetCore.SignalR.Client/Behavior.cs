using Microsoft.AspNetCore.SignalR.Client;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;

namespace Stateflows.Transport.AspNetCore.SignalR.Client
{
    internal class Behavior : IBehavior
    {
        private Task<HubConnection> Hub { get; }

        private async Task<HubConnection> GetHub()
        {
            var result = await Hub;

            if (result.State != HubConnectionState.Connected)
            {
                throw new TransportException("Transport failure: SignalR connection unavailable");
            }

            return result;
        }

        public BehaviorId Id { get; }

        public Behavior(Task<HubConnection> hub, BehaviorId id)
        {
            Hub = hub;
            Id = id;
        }

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Send", Id, StateflowsJsonConverter.SerializePolymorphicObject(@event));

            var result = StateflowsJsonConverter.DeserializeObject<SendResult>(resultString);

            return new SendResult(@event, result.Status, result.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Request", Id, StateflowsJsonConverter.SerializePolymorphicObject(request));

            var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(resultString);

            request.Respond(result.Response);

            return new RequestResult<TResponse>(request, result.Status, result.Validation);
        }
    }
}
