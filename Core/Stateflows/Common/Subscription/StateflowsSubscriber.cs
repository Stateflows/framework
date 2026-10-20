using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Context;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Subscription
{
    internal class StateflowsSubscriber(
        IBehaviorLocator behaviorLocator,
        INotificationsHub notificationsHub,
        CommonInterceptor commonInterceptor,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<StateflowsSubscriber> logger
    ) : IStateflowsSubscriber
    {
        public async Task PublishRangeAsync<TNotification>(BehaviorId publisherBehaviorId, IEnumerable<TNotification> notificationEvents, StateflowsContext senderContext,
            IDictionary<string, EventHeader>? headers = null)
        {
            var notificationType = typeof(TNotification);
            var ttlAttribute = notificationType.GetCustomAttribute<TimeToLiveAttribute>();
            var retainAttribute = notificationType.GetCustomAttribute<RetainAttribute>();
            var headersArray = headers?.Values.ToArray() ?? [];
            
            foreach (var notificationEvent in notificationEvents)
            {
                var notificationHeaders = headers?.ToDictionary() ?? [];

                var eventHolder = new EventHolder<TNotification>()
                {
                    Payload = notificationEvent,
                    SenderId = publisherBehaviorId,
                    SentAt = DateTime.Now,
                    Headers = notificationHeaders,
                    TimeToLive = ttlAttribute?.SecondsToLive ??
                                 headersArray.OfType<TimeToLive>().FirstOrDefault()?.SecondsToLive ?? 0,
                    Retained = retainAttribute != null || headersArray.OfType<Retain>().FirstOrDefault() != null
                };

                var context = new BehaviorActionContext(senderContext, serviceProvider)
                {
                    Headers = senderContext.ExecutionTriggerHolder!.Headers,
                    ExecutionTrigger = senderContext.ExecutionTriggerHolder!.BoxedPayload,
                    ExecutionTriggerId = senderContext.ExecutionTriggerHolder!.Id
                };

                await commonInterceptor.NotificationPublishedAsync(context, notificationEvent, notificationHeaders);

                var immediately = Attribute.IsDefined(eventHolder.PayloadType, typeof(ImmediatePublishAttribute)) ||
                    notificationHeaders.Values.Any(header => header is ImmediatePublish);
                var snapshot = StateflowsJsonConverter.Clone(eventHolder);
                var subscribers = senderContext.Subscribers.TryGetValue(Event<TNotification>.Name, out var behaviorIds)
                    ? behaviorIds.ToArray()
                    : [];
                async Task DispatchAsync()
                {
                    // Delivery is queued, not awaited: a subscriber may send an event back to its publisher.
                    foreach (var id in subscribers)
                    {
                        _ = DeliverAsync(id, snapshot);
                    }
                    await notificationsHub.PublishAsync(snapshot);
                }

                if (!PublicationScope.TryDispatch(DispatchAsync, immediately))
                    await DispatchAsync();
            }
        }

        private async Task DeliverAsync<TNotification>(BehaviorId id, EventHolder<TNotification> notification)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var locator = scope.ServiceProvider.GetRequiredService<IBehaviorLocator>();
                if (locator.TryLocateBehavior(id, out var behavior))
                {
                    using (behavior)
                        await behavior.SendAsync(notification.Payload, notification.Headers);
                }
                else
                {
                    logger.LogWarning("Cannot deliver notification {NotificationName} to behavior {BehaviorId}.", notification.Name, id);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to deliver notification {NotificationName} to behavior {BehaviorId}.", notification.Name, id);
            }
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
