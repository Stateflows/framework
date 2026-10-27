using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Publishes, handles, and retrieves event notifications.</summary>
    public interface INotificationsHub
    {
        /// <summary>Publishes one event envelope as a notification.</summary>
        /// <param name="eventHolder">The envelope to publish.</param>
        /// <returns>A task that completes when publication has finished.</returns>
        Task PublishAsync(EventHolder eventHolder);
        
        /// <summary>Publishes a collection of event envelopes as notifications.</summary>
        /// <param name="eventHolders">The envelopes to publish.</param>
        /// <returns>A task that completes when publication has finished.</returns>
        Task PublishRangeAsync(IEnumerable<EventHolder> eventHolders);

        /// <summary>Registers a handler to receive notifications.</summary>
        /// <param name="notificationHandler">The handler to register.</param>
        void RegisterHandler(INotificationHandler notificationHandler);

        /// <summary>Removes a previously registered notification handler.</summary>
        /// <param name="notificationHandler">The handler to unregister.</param>
        void UnregisterHandler(INotificationHandler notificationHandler);
        
        /// <summary>Retrieves notifications for a behavior by event name.</summary>
        /// <param name="behaviorId">The behavior whose notifications are queried.</param>
        /// <param name="notificationNames">The event names to retrieve.</param>
        /// <param name="lastNotificationCheck">An optional lower time bound.</param>
        /// <returns>The matching notification envelopes.</returns>
        Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime? lastNotificationCheck = null);
        
        /// <summary>Retrieves notifications for a behavior by payload type.</summary>
        /// <typeparam name="TNotification">The notification payload type.</typeparam>
        /// <param name="behaviorId">The behavior whose notifications are queried.</param>
        /// <param name="lastNotificationCheck">An optional lower time bound.</param>
        /// <returns>The notification payloads matching the requested type.</returns>
        Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(BehaviorId behaviorId, DateTime? lastNotificationCheck = null);
    }
}
