namespace Stateflows.Interfaces;

[Alias("Stateflows.Interfaces.INotificationsGrain")]
internal interface INotificationsGrain : IGrainWithStringKey
{
    [Alias("PublishAsync")]
    Task PublishAsync(OrleansEventHolder[] notifications);
    
    [Alias("GetNotificationsAsync")]
    Task<OrleansEventHolder[]> GetNotificationsAsync(DateTime? lastNotificationsCheck = null, string[]? notificationNames = null);

    [Alias("AddSubscriptionAsync")]
    Task AddSubscriptionAsync(OrleansBehaviorId behaviorId, string[] notificationNames);
    [Alias("RemoveSubscriptionAsync")]
    Task RemoveSubscriptionAsync(OrleansBehaviorId behaviorId, string[]? notificationNames = null);
    
    [Alias("AddWatchAsync")]
    Task AddWatchAsync(IBehaviorGrainObserver observer, string[] notificationNames);
    [Alias("RemoveWatchAsync")]
    Task RemoveWatchAsync(IBehaviorGrainObserver observer, string[]? notificationNames = null);
}