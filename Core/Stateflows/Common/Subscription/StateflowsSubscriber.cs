using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Subscription
{
    internal class StateflowsSubscriber(
        IBehaviorLocator behaviorLocator,
        INotificationsHub notificationsHub,
        CommonInterceptor commonInterceptor,
        IServiceProvider serviceProvider
    ) : IStateflowsSubscriber
    {
        public async Task PublishAsync<TNotification>(BehaviorId publisherBehaviorId, TNotification notificationEvent, StateflowsContext senderContext,
            IDictionary<string, EventHeader>? headers = null)
        {
            var notificationType = typeof(TNotification);
            var ttlAttribute = notificationType.GetCustomAttribute<TimeToLiveAttribute>();
            var retainAttribute = notificationType.GetCustomAttribute<RetainAttribute>();
            headers = headers?.ToDictionary() ?? [];
            var headersArray = headers?.Values.ToArray() ?? [];
            var eventHolder = new EventHolder<TNotification>()
            {
                Payload = notificationEvent,
                SenderId = publisherBehaviorId,
                SentAt = DateTime.Now,
                Headers = headers as Dictionary<string, EventHeader>,
                TimeToLive = ttlAttribute?.SecondsToLive ?? headersArray.OfType<TimeToLive>().FirstOrDefault()?.SecondsToLive ?? 0,
                Retained = retainAttribute != null || headersArray.OfType<Retain>().FirstOrDefault() != null
            };

            var context = new BehaviorActionContext(senderContext, serviceProvider)
            {
                Headers = senderContext.ExecutionTriggerHolder!.Headers,
                ExecutionTrigger = senderContext.ExecutionTriggerHolder!.BoxedPayload,
                ExecutionTriggerId = senderContext.ExecutionTriggerHolder!.Id
            };
            
            await commonInterceptor.NotificationPublishedAsync(context, notificationEvent, headers!);

            if (senderContext.Subscribers.TryGetValue(Event<TNotification>.Name, out var behaviorIds))
            {
                _ = Task.WhenAll(
                    behaviorIds.Select(
                        id => behaviorLocator.TryLocateBehavior(id, out var behavior)
                            ? behavior.SendAsync(notificationEvent)
                            : Task.CompletedTask
                    )
                );
            }

            await notificationsHub.PublishAsync(eventHolder);
        }

        public Task SubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId)
        {
            var request = new Subscribe() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(
                subscribedBehaviorId,
                out var behavior
            )
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(EventStatus.Undelivered, new EventValidation(true))
                );
        }

        public Task UnsubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId)
        {
            var request = new Unsubscribe() { BehaviorId = subscriberBehaviorId };

            request.NotificationNames.Add(typeof(TNotification).GetEventName());

            return behaviorLocator.TryLocateBehavior(
                subscribedBehaviorId,
                out var behavior
            )
                ? behavior.SendAsync(request)
                : Task.FromResult(
                    new SendResult(EventStatus.Undelivered, new EventValidation(true))
                );
        }
    }
}
