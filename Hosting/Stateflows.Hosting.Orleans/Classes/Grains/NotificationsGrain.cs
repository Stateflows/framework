using Microsoft.Extensions.Logging;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

internal class NotificationsGrain(
    [PersistentState(nameof(notificationsState), "stateflows")] IPersistentState<List<OrleansEventHolder>> notificationsState,
    [PersistentState(nameof(subscriptionsState), "stateflows")] IPersistentState<Dictionary<string, HashSet<string>>> subscriptionsState,
    IGrainFactory grainFactory,
    ILogger<NotificationsGrain> logger
) : Grain, INotificationsGrain
{
    private IGrainTimer? Timer;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        Timer = this.RegisterGrainTimer(
            static (state, ct) => state.CleanupNotificationsAsync(),
            this,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromMinutes(1)
        );
        
        return base.OnActivateAsync(cancellationToken);
    }

    public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        Timer = null;

        return base.OnDeactivateAsync(reason, cancellationToken);
    }

    private readonly WatchesManager WatchesManager = new(TimeSpan.FromMinutes(5), logger);
    
    public async Task PublishAsync(OrleansEventHolder[] notifications)
    {
        notificationsState.State.AddRange(notifications);
        
        await notificationsState.WriteStateAsync();

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

    public Task<OrleansEventHolder[]> GetNotificationsAsync(DateTime? lastNotificationsCheck = null, string[]? notificationNames = null)
    {
        lastNotificationsCheck ??= DateTime.Now;

        return Task.FromResult(notificationsState.State.Where(n =>
                (notificationNames?.Contains(n.Name) ?? true) &&
                (
                    n.SentAt.AddSeconds(n.TimeToLive) >= lastNotificationsCheck ||
                    n.Retained
                )
            )
            .ToArray());
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
        
        var subscriber = grainFactory.GetGrain<IBehaviorGrain>(behaviorGrainKey);
        
        foreach (var notification in notificationsState.State.Where(n => justSubscribed.Contains(n.Name) && n.Retained))
        {
            await subscriber.ProcessEventAsync(notification, CancellationToken.None);
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
        
        await observer.NotifyAsync(notificationsState.State
            .Where(n => justSubscribed.Contains(n.Name) && n.Retained)
            .ToArray()
        );
    }

    public Task RemoveWatchAsync(IBehaviorGrainObserver observer, string[]? notificationNames = null)
    {
        WatchesManager.Unwatch(observer, notificationNames);

        return Task.CompletedTask;
    }

    private async Task CleanupNotificationsAsync(bool writeState = true)
    {
        var count = notificationsState.State.Count;
        notificationsState.State = notificationsState.State
            .Where(n => n.Retained || n.SentAt.AddSeconds(n.TimeToLive) < DateTime.Now.AddMinutes(-1))
            .ToList();

        var retained = notificationsState.State
            .Where(n => n.Retained)
            .OrderBy(n => n.SentAt)
            .GroupBy(n => n.Name)
            .Select(g => g.Last().Id)
            .ToArray();
        
        notificationsState.State = notificationsState.State
            .Where(n =>
                !n.Retained ||
                retained.Contains(n.Id)
            )
            .ToList();

        if (writeState && count != notificationsState.State.Count)
        {
            await notificationsState.WriteStateAsync();
        }
    }
}