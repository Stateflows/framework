using Microsoft.Extensions.Logging;
using Orleans.Utilities;
using Stateflows.Interfaces;

namespace Stateflows;

internal class WatchesManager(TimeSpan expiration, ILogger log)
{
    private readonly ObserverManager<IBehaviorGrainObserver> Manager = new(expiration, log);
    private readonly Dictionary<IAddressable, HashSet<string>> NotificationNames = new();

    public string[] Watch(IBehaviorGrainObserver watcher, string[] notificationNames)
    {
        Manager.Subscribe(watcher, watcher);

        var justSubscribed = new HashSet<string>();
        if (!NotificationNames.TryGetValue(watcher, out var names))
        {
            names = [];
            NotificationNames.Add(watcher, names);
        }
        
        foreach (var notificationName in notificationNames)
        {
            if (names.Add(notificationName))
            {
                justSubscribed.Add(notificationName);
            }
        }

        return justSubscribed.ToArray();
    }
    
    public void Unwatch(IBehaviorGrainObserver watcher, string[]? notificationNames = null)
    {
        if (notificationNames == null)
        {
            NotificationNames.Remove(watcher);
            Manager.Unsubscribe(watcher);
            return;
        }
        
        if (NotificationNames.TryGetValue(watcher, out var names))
        {
            foreach (var notificationName in notificationNames)
            {
                names.Remove(notificationName);
            }
        }

        if ((names?.Count ?? 0) == 0)
        {
            NotificationNames.Remove(watcher);
            Manager.Unsubscribe(watcher);
        }
    }

    public async Task NotifyAsync(OrleansEventHolder[] notifications)
    {
        var notificationNames = notifications.Select(n => n.Name).ToArray();
        
        await Manager.Notify(
            w =>
            {
                var names = NotificationNames.GetValueOrDefault(w, []);
                var filtered = notifications.Where(n => names.Contains(n.Name)).ToArray();
                return w.NotifyAsync(filtered);
            },
            w => NotificationNames.TryGetValue(w, out var names) && names.Overlaps(notificationNames)
        );
    }
}
