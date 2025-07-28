using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
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

        public Task PublishAsync<TNotification>(BehaviorId behaviorId, TNotification notificationEvent, IEnumerable<EventHeader> headers = null)
        {
            var notificationType = typeof(TNotification);
            var ttlAttribute = notificationType.GetCustomAttribute<TimeToLiveAttribute>();
            var retainAttribute = notificationType.GetCustomAttribute<RetainAttribute>();
            var headersArray = headers?.ToArray() ?? new EventHeader[] { };
            var eventHolder = new EventHolder<TNotification>()
            {
                Payload = notificationEvent,
                SenderId = behaviorId,
                SentAt = DateTime.Now,
                Headers = headersArray.ToList(),
                TimeToLive = ttlAttribute?.SecondsToLive ?? headersArray.OfType<TimeToLive>().FirstOrDefault()?.SecondsToLive ?? 0,
                Retained = retainAttribute != null || headersArray.OfType<Retain>().FirstOrDefault() != null
            };

            if (context.Subscribers.TryGetValue(Event<TNotification>.Name, out var behaviorIds))
            {
                _ = Task.WhenAll(
                    behaviorIds.Select(
                        id => behaviorLocator.TryLocateBehavior(id, out var behavior)
                            ? behavior.SendAsync(notificationEvent)
                            : Task.CompletedTask
                    )
                );
            }

            _ = subscriptionHub.PublishAsync(eventHolder);

            if (context.Relays.TryGetValue(Event<TNotification>.Name, out behaviorIds))
            {
                _ = Task.WhenAll(
                    behaviorIds.Select(
                        id =>
                        {
                            var relayedEventHolder = new EventHolder<TNotification>()
                            {
                                Payload = eventHolder.Payload,
                                SenderId = id,
                                SentAt = eventHolder.SentAt,
                                Headers = eventHolder.Headers,
                                TimeToLive = eventHolder.TimeToLive,
                                Retained = eventHolder.Retained,
                            };
                            
                            return subscriptionHub.PublishAsync(relayedEventHolder);
                        })
                );
            }

            return Task.CompletedTask;
        }

        public Task<SendResult> SubscribeAsync<TNotification>(BehaviorId behaviorId)
        {
            var request = new Subscribe() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(
                behaviorId,
                out var behavior
            )
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(EventStatus.Undelivered)
                );
        }

        public Task<SendResult> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
        {
            var request = new Unsubscribe() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(
                behaviorId,
                out var behavior
            )
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(EventStatus.Undelivered)
                );
        }
    }
}
