﻿using System;
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

        public async Task PublishAsync<TNotification>(BehaviorId behaviorId, TNotification notification)
            where TNotification : Notification, new()
        {
            notification.SenderId = behaviorId;
            notification.SentAt = DateTime.Now;

            if (context.Subscribers.TryGetValue(EventInfo<TNotification>.Name, out var behaviorIds))
            {
                await Task.WhenAll(
                    behaviorIds.Select(
                        behaviorId => behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                            ? behavior.SendAsync(notification)
                            : Task.CompletedTask
                    )
                );
            }

            await subscriptionHub.PublishAsync(notification);
        }

        public Task<RequestResult<SubscriptionResponse>> SubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
        {
            var request = new SubscriptionRequest() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(EventInfo<TNotification>.Name);

            return behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.RequestAsync(request)
                : Task.FromResult(
                    new RequestResult<SubscriptionResponse>(request, EventStatus.Undelivered, new EventValidation(true, Array.Empty<ValidationResult>()))
                );
        }

        public Task<RequestResult<UnsubscriptionResponse>> UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
            where TNotification : Notification, new()
        {
            var request = new UnsubscriptionRequest() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(EventInfo<TNotification>.Name);

            return behaviorLocator.TryLocateBehavior(behaviorId, out var behavior)
                ? behavior.RequestAsync(request)
                : Task.FromResult(
                    new RequestResult<UnsubscriptionResponse>(request, EventStatus.Undelivered, new EventValidation(true, Array.Empty<ValidationResult>()))
                );
        }
    }
}
