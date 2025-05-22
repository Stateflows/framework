using Microsoft.AspNetCore.SignalR;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;
using Exception = System.Exception;

namespace Stateflows.Transport.AspNetCore.SignalR
{
    public class StateflowsHub : Hub
    {
        private readonly IBehaviorLocator _locator;

        private readonly IEnumerable<IBehaviorProvider> _providers;

        private readonly INotificationsHub _hub;

        private readonly Dictionary<string, Guid> _clients = new();

        private readonly Dictionary<Guid, Dictionary<BehaviorId, IBehavior>> _behaviors = new();

        public StateflowsHub(IBehaviorLocator locator, IEnumerable<IBehaviorProvider> providers, INotificationsHub hub)
        {
            _locator = locator;
            _providers = providers;
            _hub = hub;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_clients)
            {
                _clients.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public void Greet(Guid clientId)
        {
            lock (_clients)
            {
                _clients[Context.ConnectionId] = clientId;
                if (!_behaviors.ContainsKey(clientId))
                {
                    _behaviors[clientId] = new();
                }
            }
        }

        public Task<IEnumerable<BehaviorClass>> GetAvailableClasses()
        {
            return Task.FromResult(_providers.SelectMany(p => p.BehaviorClasses));
        }

        public async Task<string> Send(BehaviorId behaviorId, string eventData)
        {
            behaviorId.BehaviorClass = behaviorId.BehaviorClass.ApplyCurrentEnvironment();

            EventHolder? @event;
            try
            {
                @event = StateflowsJsonConverter.DeserializeObject<EventHolder>(eventData);
            }
            catch (Exception e)
            {
                throw new SerializationException("Unable to parse event data", e);
            }

            if (@event == null)
            {
                throw new SerializationException("Unable to parse event data");
            }

            if (!_locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                throw new BehaviorInstanceException("Behavior not found", behaviorId);
            }

            var result = await behavior.SendAsync(@event);

            result = new RequestResult(
                // @event,
                @event.GetResponseHolder(),
                result.Status,
                await _hub.GetNotificationsAsync(behaviorId),
                result.Validation
            );

            return StateflowsJsonConverter.SerializePolymorphicObject(result, true);
        }

        public async Task<string> Request(BehaviorId behaviorId, string requestData)
        {
            EventHolder? eventHolder;
            try
            {
                eventHolder = StateflowsJsonConverter.DeserializeObject<EventHolder>(requestData);
            }
            catch (Exception e)
            {
                throw new SerializationException("Unable to parse request data", e);
            }

            if (eventHolder == null)
            {
                throw new SerializationException("Unable to parse request data");
            }

            if (!eventHolder.IsRequest())
            {
                throw new SerializationException("Request data is invalid");
            }

            if (_locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                var result = await behavior.SendAsync(eventHolder);

                result = new RequestResult(
                    // eventHolder,
                    eventHolder.GetResponseHolder(),
                    result.Status,
                    await _hub.GetNotificationsAsync(behaviorId),
                    result.Validation
                );

                return StateflowsJsonConverter.SerializePolymorphicObject(result, true);
            }
            else
            {
                throw new BehaviorInstanceException("Behavior not found", behaviorId);
            }
        }
    }
}