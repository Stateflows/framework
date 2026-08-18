using Microsoft.Extensions.Logging;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

internal class SubscriptionsGrain(
    [PersistentState(nameof(retainedNotificationsState), "stateflows")]
    IPersistentState<Dictionary<string, OrleansEventHolder>> retainedNotificationsState,
    [PersistentState(nameof(subscriptionsState), "stateflows")]
    IPersistentState<Dictionary<string, HashSet<string>>> subscriptionsState,
    IGrainFactory grainFactory,
    ILogger<SubscriptionsGrain> logger
) : Grain, ISubscriptionsGrain
{
    private readonly WatchesManager WatchesManager = new(TimeSpan.FromMinutes(5), logger);
    
    public async Task PublishAsync(OrleansEventHolder[] notifications)
    {
        var retainedNotifications = notifications.Where(n => n.Retained).ToArray();
        foreach (var retainedNotification in retainedNotifications)
        {
            retainedNotificationsState.State[retainedNotification.Name] = retainedNotification;
        }
        await retainedNotificationsState.WriteStateAsync();

        await WatchesManager.NotifyAsync(notifications);

        foreach (var subscription in subscriptionsState.State)
        {
            var subscriber = grainFactory.GetGrain<IBehaviorGrain>(subscription.Key);

            foreach (var notification in notifications.Where(n => subscription.Value.Contains(n.Name)))
            {
                await subscriber.ProcessEventAsync(notification, CancellationToken.None);
            }
        }
    }

    public async Task AddSubscriptionAsync(OrleansBehaviorId behaviorId, string[] notificationNames)
    {
        var behaviorGrainKey = StateflowsJsonConverter.SerializeObject(behaviorId);
        var justSubscribed = new HashSet<string>();
        if (!subscriptionsState.State.TryGetValue(behaviorGrainKey, out var subscribedNotificationNames))
        {
            subscribedNotificationNames = [];
            subscriptionsState.State.Add(behaviorGrainKey, subscribedNotificationNames);
        }

        foreach (var notificationName in notificationNames)
        {
            if (subscribedNotificationNames.Add(notificationName))
            {
                justSubscribed.Add(notificationName);
            }
        }

        await subscriptionsState.WriteStateAsync();

        var retainedNotifications = retainedNotificationsState.State
            .Where(kv => justSubscribed.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToArray();

        if (retainedNotifications.Any())
        {
            var subscriber = grainFactory.GetGrain<IBehaviorGrain>(behaviorGrainKey);
            
            foreach (var retainedNotification in retainedNotifications)
            {
                await subscriber.ProcessEventAsync(retainedNotification, CancellationToken.None);
            }
        }
    }

    public Task RemoveSubscriptionAsync(OrleansBehaviorId behaviorId, string[]? notificationNames = null)
    {
        var behaviorGrainKey = StateflowsJsonConverter.SerializeObject(behaviorId);
        if (notificationNames == null)
        {
            subscriptionsState.State.Remove(behaviorGrainKey);
        }
        else
        {
            if (subscriptionsState.State.TryGetValue(behaviorGrainKey, out var subscribedNotificationNames))
            {
                foreach (var notificationName in notificationNames)
                {
                    subscribedNotificationNames.Remove(notificationName);
                }
            }

            if ((subscribedNotificationNames?.Count ?? 0) == 0)
            {
                subscriptionsState.State.Remove(behaviorGrainKey);
            }
        }
        
        return subscriptionsState.WriteStateAsync();
    }

    public async Task AddWatchAsync(IBehaviorGrainObserver observer, string[] notificationNames)
    {
        var justSubscribed = WatchesManager.Watch(observer, notificationNames);

        var retainedNotifications = retainedNotificationsState.State
            .Where(kv => justSubscribed.Contains(kv.Key))
            .Select(kv => kv.Value)
            .ToArray();

        if (retainedNotifications.Any())
        {
            await observer.NotifyAsync(retainedNotifications);
        }
    }

    public Task RemoveWatchAsync(IBehaviorGrainObserver observer, string[]? notificationNames = null)
    {
        WatchesManager.Unwatch(observer, notificationNames);

        return Task.CompletedTask;
    }
}