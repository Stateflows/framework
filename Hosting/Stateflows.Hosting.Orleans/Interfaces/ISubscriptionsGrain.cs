using Orleans.Concurrency;

namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.ISubscriptionsGrain")]
internal interface ISubscriptionsGrain : IGrainWithStringKey, IRemindable
{
    /// <summary>
    /// Durably stores notifications and schedules deliveries; returns after state commit and reminder registration.
    /// </summary>
    [Alias("PublishAsync")]
    Task PublishAsync(OrleansEventHolder[] notifications);

    /// <summary>
    /// Returns latest non-expired retained notifications for given names unioned with non-expired notifications sent since given moment.
    /// </summary>
    [Alias("GetNotificationsAsync")]
    Task<OrleansEventHolder[]> GetNotificationsAsync(DateTime? since, string[] notificationNames);

    [Alias("AddSubscriptionWithReplayAsync")]
    Task AddSubscriptionAsync(OrleansBehaviorId behaviorId, string[] notificationNames, DateTime? replayNotificationsSince = null);
    [Alias("RemoveSubscriptionAsync")]
    Task RemoveSubscriptionAsync(OrleansBehaviorId behaviorId, string[]? notificationNames = null);
    
    /// <summary>
    /// Registers observer for given notification names and returns replay (latest retained plus notifications since given moment),
    /// which is also pushed to the observer in background. Initial registration (<paramref name="renewal"/> = false) replays all
    /// requested names; renewal replays only names this grain activation did not know for the observer (e.g. after reactivation).
    /// </summary>
    [Alias("AddWatchWithRenewalAsync")]
    Task<OrleansEventHolder[]> AddWatchAsync(IBehaviorGrainObserver observer, string[] notificationNames, DateTime? replayNotificationsSince = null, bool renewal = false);
    [Alias("RemoveWatchAsync")]
    Task RemoveWatchAsync(IBehaviorGrainObserver observer, string[]? notificationNames = null);

    [Alias("GetPendingDeliveriesAsync")]
    Task<NotificationDelivery[]> GetPendingDeliveriesAsync(string subscriberGrainKey, int maxCount);
    /// <summary>
    /// Read-only check whether delivery is still pending, its notification is not expired and the subscriber is still
    /// subscribed to it. Used by delivery workers right before dispatching each delivery.
    /// </summary>
    [ReadOnly]
    [Alias("IsDeliveryPendingAsync")]
    Task<bool> IsDeliveryPendingAsync(string subscriberGrainKey, Guid deliveryId);
    [Alias("ConfirmDeliveriesAsync")]
    Task ConfirmDeliveriesAsync(string subscriberGrainKey, Guid[] deliveryIds);
}
