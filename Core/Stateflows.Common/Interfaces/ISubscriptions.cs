using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface ISubscriptions
    {
        /// <summary>
        /// Subscribes for notifications from given behavior (by sending <see cref="Subscribe"/> to it).<br/>
        /// Subscription is durable over time; the only way to end it is by calling <see cref="UnsubscribeAsync"/> with same parameter.
        /// </summary>
        /// <typeparam name="TNotificationEvent">Subscribed notification type</typeparam>
        /// <param name="behaviorId">Identifier of a behavior being subscribed to</param>
        /// <returns>Task providing <see cref="SubscriptionResponse"/></returns>
        Task<SendResult> SubscribeAsync<TNotificationEvent>(BehaviorId behaviorId);

        /// <summary>
        /// Unsubscribes for notifications from given behavior (by sending <see cref="Unsubscribe"/> to it).
        /// </summary>
        /// <typeparam name="TNotificationEvent">Unsubscribed notification type</typeparam>
        /// <param name="behaviorId">Identifier of a behavior being unsubscribed to</param>
        /// <returns>Task providing <see cref="UnsubscriptionResponse"/></returns>
        Task<SendResult> UnsubscribeAsync<TNotificationEvent>(BehaviorId behaviorId);
    }
}