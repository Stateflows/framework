using Microsoft.AspNetCore.SignalR.Client;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Extensions;
using Stateflows.Common.Exceptions;

namespace Stateflows.Transport.SignalR.Client
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

        {
            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Send", Id, StateflowsJsonConverter.SerializePolymorphicObject(@event, true));

            var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(resultString);

            if (result.Response != null)
            {
                @event.Respond(result.Response);
            }

            return new SendResult(@event, result.Status, result.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(Request<TResponse> request)
            where TResponse : IResponse, new()
        {
            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Request", Id, StateflowsJsonConverter.SerializePolymorphicObject(request, true));

            var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(resultString);

            request.Respond(result.Response);

            return new RequestResult<TResponse>(request, result.Status, result.Validation);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }

        public Task WatchAsync<TNotification>(Action<TNotification> handler) where TNotification : Notification, new()
        {
            throw new NotImplementedException();
        }

        public Task UnwatchAsync<TNotification>() where TNotification : Notification, new()
        {
            throw new NotImplementedException();
        }

        ~Behavior()
            => Dispose(false);
    }
}
