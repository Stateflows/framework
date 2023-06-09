﻿using Microsoft.AspNetCore.SignalR;
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

        public async Task<bool> Send(BehaviorId behaviorId, string eventData)
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

            return await behavior.SendAsync(@event);
        }

        public async Task<Response> Request(BehaviorId behaviorId, string requestData)
        {
            Event? @event = null;
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
                var consumed = await behavior.SendAsync(@event);
                return @event.GetResponse();
            }
            else
            {
                throw new Exception("Behavior not found");
            }
        }
    }
}