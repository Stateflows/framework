using Microsoft.AspNetCore.SignalR.Client;
using Stateflows.Common;
using Stateflows.Common.Utilities;
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

        public async Task<SendResult> SendAsync<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null)
        {
            var eventHolder = @event.ToTypedEventHolder();
            eventHolder.Headers.AddRange(headers);

            ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Send", Id, StateflowsJsonConverter.SerializePolymorphicObject(eventHolder, true));

            var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(resultString);

            return new SendResult(eventHolder, result.Status, Array.Empty<EventHolder>(), result.Validation);
        }

        public async Task<RequestResult<TResponse>> RequestAsync<TResponse>(IRequest<TResponse> request, IEnumerable<EventHeader> headers = null)
        {
            var requestHolder = request.ToTypedEventHolder();
            requestHolder.Headers.AddRange(headers);

            ResponseHolder.SetResponses(new Dictionary<object, EventHolder>());

            var hub = await GetHub();

            var resultString = await hub.InvokeAsync<string>("Request", Id, StateflowsJsonConverter.SerializePolymorphicObject(request, true));

            var result = StateflowsJsonConverter.DeserializeObject<RequestResult>(resultString);

            return new RequestResult<TResponse>(requestHolder, result.Status, result.Validation);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }

        public Task<IWatcher> WatchAsync<TNotification>(Action<TNotification> handler)
        {
            throw new NotImplementedException();
        }

        public Task<IWatcher> UnwatchAsync<TNotification>()
        {
            throw new NotImplementedException();
        }

        ~Behavior()
            => Dispose(false);
    }
}
