using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Creates and removes durable notification subscriptions on behalf of a behavior context.</summary>
    /// <typeparam name="TReturn">The context type returned by related messaging operations.</typeparam>
    public interface ISubscriptions<TReturn>
    {
        /// <summary>
        /// Subscribes for notifications from given behavior (by sending <see cref="Subscribe"/> to it).<br/>
        /// Subscription is durable over time; the only way to end it is by calling <see cref="UnsubscribeAsync{TNotification}(BehaviorId)"/> with the same behavior and notification type.
        /// </summary>
        /// <typeparam name="TNotification">Subscribed notification type</typeparam>
        /// <param name="behaviorId">Identifier of a behavior being subscribed to</param>
        /// <returns>A task that completes when the durable subscription request has been sent.</returns>
        Task SubscribeAsync<TNotification>(BehaviorId behaviorId);

        /// <summary>
        /// Unsubscribes for notifications from given behavior (by sending <see cref="Unsubscribe"/> to it).
        /// </summary>
        /// <typeparam name="TNotification">Unsubscribed notification type</typeparam>
        /// <param name="behaviorId">Identifier of a behavior being unsubscribed to</param>
        /// <returns>A task that completes when the unsubscribe request has been sent.</returns>
        Task UnsubscribeAsync<TNotification>(BehaviorId behaviorId);
    }
}