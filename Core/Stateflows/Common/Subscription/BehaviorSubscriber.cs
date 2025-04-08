using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Stateflows.Common.Context;

namespace Stateflows.Common.Subscription
{
    internal class BehaviorSubscriber
    {
        private readonly StateflowsContext context;
        private readonly BehaviorId subscriberBehaviorId;
        private readonly IBehaviorLocator behaviorLocator;
        private readonly NotificationsHub subscriptionHub;
        public BehaviorSubscriber(BehaviorId behaviorId, StateflowsContext context, IBehaviorLocator behaviorLocator, NotificationsHub subscriptionHub)
        {
            this.context = context;
            this.subscriberBehaviorId = behaviorId;
            this.behaviorLocator = behaviorLocator;
            this.subscriptionHub = subscriptionHub;
        }

        public async Task PublishAsync<TNotification>(BehaviorId behaviorId, TNotification notificationEvent, IEnumerable<EventHeader> headers = null, int timeToLiveInSeconds = 60)
        {
            var eventHolder = new EventHolder<TNotification>()
            {
                Payload = notificationEvent,
                SenderId = behaviorId,
                SentAt = DateTime.Now,
                Headers = headers?.ToList() ?? new List<EventHeader>(),
                TimeToLive = timeToLiveInSeconds
            };

            if (context.Subscribers.TryGetValue(Event<TNotification>.Name, out var behaviorIds))
            {
                await Task.WhenAll(
                    behaviorIds.Select(
                        behaviorId => behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                            ? behavior.SendAsync(notificationEvent)
                            : Task.CompletedTask
                    )
                );
            }

            await subscriptionHub.PublishAsync(eventHolder);
        }

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
        {
            var request = new Subscribe() { BehaviorId = subscriberBehaviorId };

            var eventHolder = new EventHolder<Subscribe>()
            {
                Payload = request,
                SenderId = behaviorId,
                SentAt = DateTime.Now,
            };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(
                        eventHolder,
                        EventStatus.Undelivered,
                        null, // todo: get notifications
                        new EventValidation(true, Array.Empty<ValidationResult>())
                    )
                );
        }

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
        {
            var request = new Unsubscribe() { BehaviorId = subscriberBehaviorId };

            var eventHolder = new EventHolder<Unsubscribe>()
            {
                Payload = request,
                SenderId = behaviorId,
                SentAt = DateTime.Now,
            };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(
                        eventHolder,
                        EventStatus.Undelivered,
                        null,
                        new EventValidation(true, Array.Empty<ValidationResult>())
                    )
                );
        }
    }
}
