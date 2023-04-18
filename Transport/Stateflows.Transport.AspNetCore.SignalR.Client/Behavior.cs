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

        private BehaviorId Id { get; }

        public Behavior(Task<HubConnection> hub, BehaviorId id)
        {
            Hub = hub;
            Id = id;
        }

        public async Task<TResponse> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
            => await (await GetHub()).InvokeAsync<TResponse>("Request", Id, StateflowsJsonConverter.SerializeObject(request));

        public async Task<bool> SendAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => await(await GetHub()).InvokeAsync<bool>("Send", Id, StateflowsJsonConverter.SerializeObject(@event));
    }
}
