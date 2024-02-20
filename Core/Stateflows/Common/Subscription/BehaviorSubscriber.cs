using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Stateflows.Common.Context;

namespace Stateflows.Common.Subscription
{
    internal class BehaviorSubscriber
    {
        private readonly BehaviorId id;
        private readonly StateflowsContext context;
        private readonly IBehaviorLocator locator;
        public BehaviorSubscriber(BehaviorId id, StateflowsContext context, IBehaviorLocator locator)
        {
            this.id = id;
            this.context = context;
            this.locator = locator;
        }

        public void PublishAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
            => _ = context.Subscribers.TryGetValue(EventInfo<TEvent>.Name, out var behaviorIds)
                ? Task.WhenAll(
                    behaviorIds.Select(
                        behaviorId => locator.TryLocateBehavior(behaviorId, out var behavior)
                            ? behavior.SendAsync(@event)
                            : Task.CompletedTask
                    )
                )
                : Task.CompletedTask;

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new()
        {
            var request = new SubscriptionRequest()
            {
                BehaviorId = id,
                EventName = EventInfo<TEvent>.Name
            };

            return locator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.RequestAsync(request)
                : Task.FromResult(
                    new RequestResult<SubscriptionResponse>(request, EventStatus.Undelivered, new EventValidation(true, Array.Empty<ValidationResult>()))
                );
        }

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TEvent>(BehaviorId behaviorId)
            where TEvent : Event, new()
        {
            var request = new UnsubscriptionRequest()
            {
                BehaviorId = id,
                EventName = EventInfo<TEvent>.Name
            };

            return locator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.RequestAsync(request)
                : Task.FromResult(
                    new RequestResult<UnsubscriptionResponse>(request, EventStatus.Undelivered, new EventValidation(true, Array.Empty<ValidationResult>()))
                );
        }
    }
}
