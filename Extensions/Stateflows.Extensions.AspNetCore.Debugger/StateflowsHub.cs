using Microsoft.AspNetCore.SignalR;
using Stateflows.Common.Events;
using Stateflows.Common.Interfaces;

namespace Stateflows.Transport.AspNetCore.SignalR
{
    public class StateflowsHub : Hub
    {
        private IBehaviorLocator Locator { get; }

        public StateflowsHub(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public async Task Send<TEvent>(BehaviorId id, TEvent @event)
            where TEvent : Event, new()
        {
            if (Locator.TryLocateBehavior(id, out var behavior))
            {
                await behavior.Send(@event);
            }
        }

        public async Task<TResponse?> Request<TResponse>(BehaviorId id, Request<TResponse> request)
            where TResponse : Response, new()
            => Locator.TryLocateBehavior(id, out var behavior)
                ? await behavior.Request<TResponse>(request)
                : null;
    }
}