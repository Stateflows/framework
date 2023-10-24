using Microsoft.AspNetCore.SignalR.Client;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

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
                throw new Exception("Transport failure: SignalR connection unavailable");
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
            where TEvent : Event
        {
            var hub = await GetHub();

            var result = await hub.InvokeAsync<SendResult>("Send", Id, StateflowsJsonConverter.SerializeObject(@event));

            return result;
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response
        {
            var hub = await GetHub();

            var result = await hub.InvokeAsync<RequestResult<TResponse>>("Request", Id, StateflowsJsonConverter.SerializeObject(request));

            request.Respond(result.Response);

            return result;
        }
    }
}
