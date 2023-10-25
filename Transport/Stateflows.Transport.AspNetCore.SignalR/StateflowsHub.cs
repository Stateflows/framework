using Microsoft.AspNetCore.SignalR;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Extensions;

namespace Stateflows.Transport.AspNetCore.SignalR
{
    public class StateflowsHub : Hub
    {
        private readonly IBehaviorLocator _locator;

        private readonly IEnumerable<IBehaviorProvider> _providers;

        public StateflowsHub(IBehaviorLocator locator, IEnumerable<IBehaviorProvider> providers)
        {
            _locator = locator;
            _providers = providers;
        }

        public Task<IEnumerable<BehaviorClass>> GetAvailableClasses()
        {
            return Task.FromResult(_providers.SelectMany(p => p.BehaviorClasses));
        }

        public async Task<string> Send(BehaviorId behaviorId, string eventData)
        {
            Event? @event;
            try
            {
                @event = StateflowsJsonConverter.DeserializeObject<Event>(eventData);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to parse event data", e);
            }

            if (@event == null)
            {
                throw new Exception("Unable to parse event data");
            }

            if (!_locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                throw new Exception("Behavior not found");
            }

            var result = await behavior.SendAsync(@event);

            result = new RequestResult(@event, @event.GetResponse(), result.Status, result.Validation);

            return StateflowsJsonConverter.SerializeObject(result);
        }

        public async Task<string> Request(BehaviorId behaviorId, string requestData)
        {
            Event? @event;
            try
            {
                @event = StateflowsJsonConverter.DeserializeObject<Event>(requestData);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to parse request data", e);
            }

            if (@event == null)
            {
                throw new Exception("Unable to parse request data");
            }

            if (!@event.IsRequest())
            {
                throw new Exception("Request data is invalid");
            }

            if (_locator.TryLocateBehavior(behaviorId, out var behavior))
            {
                var result = await behavior.SendAsync(@event);

                result = new RequestResult(@event, @event.GetResponse(), result.Status, result.Validation);

                return StateflowsJsonConverter.SerializeObject(result);
            }
            else
            {
                throw new Exception("Behavior not found");
            }
        }
    }
}