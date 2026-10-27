using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context;

namespace Stateflows.Common.Interfaces;

/// <summary>Publishes notifications and manages behavior-to-behavior subscriptions.</summary>
public interface IStateflowsSubscriber
{
    /// <summary>Publishes a batch of notifications on behalf of a behavior.</summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="publisherBehaviorId">The behavior publishing the notifications.</param>
    /// <param name="notificationEvents">The notification payloads to publish.</param>
    /// <param name="senderContext">The context of the publishing behavior.</param>
    /// <param name="headers">Optional headers applied to the notification deliveries.</param>
    /// <returns>A task that completes when publication has finished.</returns>
    Task PublishRangeAsync<TNotification>(BehaviorId publisherBehaviorId, IEnumerable<TNotification> notificationEvents, StateflowsContext senderContext, IDictionary<string, EventHeader>? headers = null);
    /// <summary>Subscribes a behavior to notifications of a specified type from another behavior.</summary>
    /// <typeparam name="TNotification">The notification type to receive.</typeparam>
    /// <param name="subscriberBehaviorId">The behavior that will receive notifications.</param>
    /// <param name="subscribedBehaviorId">The behavior whose notifications are observed.</param>
    /// <returns>A task that completes when the subscription is established.</returns>
    Task SubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
    /// <summary>Removes a behavior's subscription to notifications from another behavior.</summary>
    /// <typeparam name="TNotification">The notification type subscription to remove.</typeparam>
    /// <param name="subscriberBehaviorId">The behavior that receives notifications.</param>
    /// <param name="subscribedBehaviorId">The behavior whose notifications are observed.</param>
    /// <returns>A task that completes when the subscription is removed.</returns>
    Task UnsubscribeAsync<TNotification>(BehaviorId subscriberBehaviorId, BehaviorId subscribedBehaviorId);
}