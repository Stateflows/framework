using Microsoft.Extensions.Logging;
using Stateflows.Common.Utilities;
using Stateflows.Interfaces;
using Stateflows.Storage;

namespace Stateflows;

/// <summary>
/// Publisher-side notification hub. Durably stores notification history, subscriptions and pending deliveries in
/// a single persistent state; deliveries are performed by <see cref="StateflowsNotificationDelivery"/> workers
/// (one per publisher/subscriber pair) and confirmed back, so this grain never awaits behavior processing.
/// </summary>
internal class StateflowsSubscriptions(
    [StateflowsPersistentState(StateflowsStateNames.Notifications)]
    IPersistentState<NotificationsState> notificationsState,
    [StateflowsPersistentState(StateflowsStateNames.RetainedNotifications)]
    IPersistentState<Dictionary<string, OrleansEventHolder>> retainedNotificationsState,
    [StateflowsPersistentState(StateflowsStateNames.Subscriptions)]
    IPersistentState<Dictionary<string, HashSet<string>>> subscriptionsState,
    IGrainFactory grainFactory,
    ILogger<StateflowsSubscriptions> logger
) : Grain, ISubscriptionsGrain
{
    public const string ReminderName = "Stateflows.Notifications";
    internal static readonly TimeSpan ReminderPeriod = TimeSpan.FromMinutes(1);
    internal static readonly TimeSpan WatchExpiration = TimeSpan.FromMinutes(5);
    private const int MaxPendingBatch = 500;

    private sealed class WatchEntry
    {
        public HashSet<string> Names { get; } = [];
        public DateTime LastSeenUtc { get; set; }
        public Task Tail { get; set; } = Task.CompletedTask;
    }

    private readonly Dictionary<IBehaviorGrainObserver, WatchEntry> watches = new();
    private bool reminderRegistered;

    private NotificationsState State => notificationsState.State;

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!notificationsState.RecordExists || notificationsState.State == null)
        {
            var state = new NotificationsState();
            if (retainedNotificationsState.RecordExists || subscriptionsState.RecordExists)
            {
                ImportLegacyState(state);
                // Throws on failure: activation fails and legacy records stay untouched, so nothing is lost
                await EnsureReminderAsync(state);
                await CommitAsync(state);
                logger.LogInformation(
                    "Migrated {RetainedCount} retained notifications and {SubscriberCount} subscribers of {Publisher} to durable notifications state.",
                    state.History.Count, state.Subscriptions.Count, this.GetPrimaryKeyString());
            }
            else
            {
                notificationsState.State = state;
            }
        }
        else
        {
            Normalize(notificationsState.State);
        }

        if (State.LegacyMigrated && (retainedNotificationsState.RecordExists || subscriptionsState.RecordExists))
        {
            await ClearLegacyStateAsync();
        }

        // Committed pending/expiring state without a reminder would be stranded; fail activation if it cannot be repaired
        if (NeedsReminder(State))
        {
            reminderRegistered = await this.GetReminder(ReminderName) != null;
            await EnsureReminderAsync(State);
        }

        KickAllPendingDeliveries();

        await base.OnActivateAsync(cancellationToken);
    }

    private void ImportLegacyState(NotificationsState state)
    {
        var retained = (retainedNotificationsState.State ?? [])
            .Where(p => !string.IsNullOrEmpty(p.Value.Name))
            .Select(p => p.Value)
            .OrderBy(n => n.SentAt.ToUniversalTime());

        foreach (var notification in retained)
        {
            var stored = new StoredNotification
            {
                Sequence = ++state.LastSequence,
                Notification = notification with { Retained = true }
            };
            state.History.Add(stored);
            state.LatestRetained[notification.Name] = stored.Sequence;
        }

        foreach (var (subscriberKey, names) in subscriptionsState.State ?? [])
        {
            if (names is { Count: > 0 })
            {
                state.Subscriptions[subscriberKey] = new HashSet<string>(names);
            }
        }

        state.LegacyMigrated = true;
    }

    private async Task ClearLegacyStateAsync()
    {
        try
        {
            if (retainedNotificationsState.RecordExists)
            {
                await retainedNotificationsState.ClearStateAsync();
            }

            if (subscriptionsState.RecordExists)
            {
                await subscriptionsState.ClearStateAsync();
            }
        }
        catch (Exception exception)
        {
            // Data is already migrated; clearing is retried on next activation
            logger.LogWarning(exception, "Failed to clear legacy notifications state of {Publisher}.", this.GetPrimaryKeyString());
        }
    }

    private static void Normalize(NotificationsState state)
    {
        state.History ??= [];
        state.Subscriptions ??= [];
        state.Pending ??= [];
        state.LatestRetained ??= [];
        state.PublicationWatermarks ??= [];
    }

    private string GetBehaviorGrainKey(OrleansBehaviorId behaviorId)
    {
        var publisher = StateflowsGrainKeys.ParseBehavior(this.GetPrimaryKeyString());
        return StateflowsGrainKeys.ForBehavior(publisher.TenantId, behaviorId);
    }

    private async Task CommitAsync(NotificationsState next)
    {
        var previous = notificationsState.State;
        notificationsState.State = next;
        try
        {
            await notificationsState.WriteStateAsync();
        }
        catch
        {
            StateflowsOrleansDiagnostics.PersistenceFailed("Stateflows.Notifications");
            notificationsState.State = previous;
            // The provider may have committed before reporting failure; reload on next activation
            DeactivateOnIdle();
            throw;
        }
    }

    #region Publication

    public Task PublishAsync(OrleansEventHolder[] notifications)
        => PublishCoreAsync(notifications);

    public Task PublishOutboxAsync(string sourceKey, long sequence, OrleansEventHolder[] notifications)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceKey);
        if (sequence <= 0 || notifications == null || notifications.Length == 0 ||
            notifications.Any(notification => string.IsNullOrEmpty(notification.Name)))
            throw new ArgumentException("An outbox publication requires a positive sequence and named notifications.");
        return PublishCoreAsync(notifications, sourceKey, sequence);
    }

    private async Task PublishCoreAsync(OrleansEventHolder[] notifications, string? sourceKey = null, long sequence = 0)
    {
        if (sourceKey != null && State.PublicationWatermarks.TryGetValue(sourceKey, out var accepted) && accepted >= sequence)
            return;

        if (notifications == null || notifications.Length == 0)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var next = State.Clone();
        var published = new List<StoredNotification>(notifications.Length);
        var subscribersToKick = new HashSet<string>();

        foreach (var notification in notifications)
        {
            if (string.IsNullOrEmpty(notification.Name))
            {
                logger.LogWarning("Skipping notification without name published by {Publisher}.", this.GetPrimaryKeyString());
                continue;
            }

            var stored = new StoredNotification
            {
                Sequence = ++next.LastSequence,
                Notification = notification
            };
            next.History.Add(stored);
            if (notification.Retained)
            {
                next.LatestRetained[notification.Name] = stored.Sequence;
            }

            if (stored.IsExpired(nowUtc))
            {
                continue;
            }

            published.Add(stored);
            foreach (var (subscriberKey, names) in next.Subscriptions)
            {
                if (names.Contains(notification.Name))
                {
                    AddPending(next, subscriberKey, stored, nowUtc);
                    subscribersToKick.Add(subscriberKey);
                }
            }
        }

        if (next.LastSequence == State.LastSequence)
        {
            return;
        }

        PruneExpired(next, nowUtc);
        if (sourceKey != null)
            next.PublicationWatermarks[sourceKey] = sequence;

        // Register first: an orphan reminder is harmless, committed deliveries without a reminder are not
        await EnsureReminderAsync(next);
        await CommitAsync(next);

        NotifyWatchers(published, nowUtc);
        foreach (var subscriberKey in subscribersToKick)
        {
            KickDelivery(subscriberKey);
        }
    }

    public Task<OrleansEventHolder[]> GetNotificationsAsync(DateTime? since, string[] notificationNames)
    {
        var names = ValidateNames(notificationNames);
        return Task.FromResult(Query(State, names, since, DateTime.UtcNow).Select(s => s.Notification).ToArray());
    }

    private static List<StoredNotification> Query(NotificationsState state, IReadOnlyCollection<string> names, DateTime? since, DateTime nowUtc)
    {
        var result = new SortedDictionary<long, StoredNotification>();
        if (names.Count == 0)
        {
            return [];
        }

        foreach (var name in names)
        {
            if (state.LatestRetained.TryGetValue(name, out var sequence) &&
                FindBySequence(state.History, sequence) is { } retained &&
                !retained.IsExpired(nowUtc))
            {
                result[retained.Sequence] = retained;
            }
        }

        if (since != null)
        {
            var sinceUtc = since.Value.ToUniversalTime();
            var nameSet = names as ISet<string> ?? names.ToHashSet();
            foreach (var stored in state.History)
            {
                if (nameSet.Contains(stored.Notification.Name) &&
                    stored.Notification.SentAt.ToUniversalTime() >= sinceUtc &&
                    !stored.IsExpired(nowUtc))
                {
                    result[stored.Sequence] = stored;
                }
            }
        }

        return result.Values.ToList();
    }

    private static StoredNotification? FindBySequence(List<StoredNotification> history, long sequence)
    {
        int low = 0, high = history.Count - 1;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            var current = history[middle].Sequence;
            if (current == sequence)
            {
                return history[middle];
            }

            if (current < sequence)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return null;
    }

    private static HashSet<string> ValidateNames(string[] notificationNames)
    {
        ArgumentNullException.ThrowIfNull(notificationNames);
        if (notificationNames.Any(string.IsNullOrEmpty))
        {
            throw new ArgumentException("Notification names must be nonempty.", nameof(notificationNames));
        }

        return notificationNames.ToHashSet();
    }

    private static void AddPending(NotificationsState state, string subscriberKey, StoredNotification stored, DateTime nowUtc)
    {
        if (!state.Pending.TryGetValue(subscriberKey, out var pending))
        {
            pending = [];
            state.Pending.Add(subscriberKey, pending);
        }
        else if (pending.Any(p => p.Sequence == stored.Sequence))
        {
            return;
        }

        pending.Add(new PendingDelivery
        {
            DeliveryId = Guid.NewGuid(),
            Sequence = stored.Sequence,
            Name = stored.Notification.Name,
            CreatedAtUtc = nowUtc
        });
    }

    private static bool PruneExpired(NotificationsState state, DateTime nowUtc)
    {
        var removed = state.History.RemoveAll(s => s.IsExpired(nowUtc)) > 0;
        if (!removed && state.Pending.Count == 0)
        {
            return false;
        }

        var changed = removed;
        foreach (var subscriberKey in state.Pending.Keys.ToArray())
        {
            var pending = state.Pending[subscriberKey];
            if (pending.RemoveAll(p => FindBySequence(state.History, p.Sequence) == null) > 0)
            {
                changed = true;
            }

            if (pending.Count == 0)
            {
                state.Pending.Remove(subscriberKey);
                changed = true;
            }
        }

        // LatestRetained intentionally keeps sequences of expired entries so older retained ones are never revived
        return changed;
    }

    private static bool NeedsReminder(NotificationsState state)
        => state.Pending.Count > 0 || state.History.Any(s => s.Notification.TimeToLive > 0);

    /// <summary>
    /// Ensures the reminder exists when <paramref name="candidate"/> state requires it. Must be called before
    /// committing the candidate state; throws if registration fails so the caller does not commit.
    /// </summary>
    private async Task EnsureReminderAsync(NotificationsState candidate)
    {
        if (reminderRegistered || !NeedsReminder(candidate))
        {
            return;
        }

        try
        {
            await this.RegisterOrUpdateReminder(ReminderName, ReminderPeriod, ReminderPeriod);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to register notifications reminder of {Publisher}; retrying.", this.GetPrimaryKeyString());
            try
            {
                await this.RegisterOrUpdateReminder(ReminderName, ReminderPeriod, ReminderPeriod);
            }
            catch (Exception retryException)
            {
                StateflowsOrleansDiagnostics.ReminderFailed("notifications.register");
                logger.LogError(retryException,
                    "Failed to register notifications reminder of {Publisher}; state change is not committed.",
                    this.GetPrimaryKeyString());
                throw;
            }
        }

        reminderRegistered = true;
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        if (reminderName != ReminderName)
        {
            return;
        }

        reminderRegistered = true;
        var nowUtc = DateTime.UtcNow;
        var next = State.Clone();
        if (PruneExpired(next, nowUtc))
        {
            try
            {
                await CommitAsync(next);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to remove expired notifications of {Publisher}.", this.GetPrimaryKeyString());
                return;
            }
        }

        KickAllPendingDeliveries();

        if (!NeedsReminder(State))
        {
            try
            {
                var reminder = await this.GetReminder(ReminderName);
                if (reminder != null)
                {
                    await this.UnregisterReminder(reminder);
                }

                reminderRegistered = false;
            }
            catch (Exception exception)
            {
                StateflowsOrleansDiagnostics.ReminderFailed("notifications.unregister");
                logger.LogWarning(exception, "Failed to unregister notifications reminder of {Publisher}.", this.GetPrimaryKeyString());
            }
        }
    }

    #endregion

    #region Subscriptions

    public async Task AddSubscriptionAsync(OrleansBehaviorId behaviorId, string[] notificationNames, DateTime? replayNotificationsSince = null)
    {
        var names = ValidateNames(notificationNames);
        var subscriberKey = GetBehaviorGrainKey(behaviorId);
        var nowUtc = DateTime.UtcNow;
        var next = State.Clone();

        if (!next.Subscriptions.TryGetValue(subscriberKey, out var subscribedNames))
        {
            subscribedNames = [];
            next.Subscriptions.Add(subscriberKey, subscribedNames);
        }

        var justSubscribed = names.Where(subscribedNames.Add).ToHashSet();
        if (justSubscribed.Count == 0)
        {
            return;
        }

        var replay = Query(next, justSubscribed, replayNotificationsSince, nowUtc);
        foreach (var stored in replay)
        {
            AddPending(next, subscriberKey, stored, nowUtc);
        }

        // Register first: an orphan reminder is harmless, committed deliveries without a reminder are not
        await EnsureReminderAsync(next);
        await CommitAsync(next);

        if (replay.Count > 0)
        {
            KickDelivery(subscriberKey);
        }
    }

    public async Task RemoveSubscriptionAsync(OrleansBehaviorId behaviorId, string[]? notificationNames = null)
    {
        var subscriberKey = GetBehaviorGrainKey(behaviorId);
        var next = State.Clone();
        var changed = false;

        if (next.Subscriptions.TryGetValue(subscriberKey, out var subscribedNames))
        {
            if (notificationNames == null)
            {
                subscribedNames.Clear();
                changed = true;
            }
            else
            {
                foreach (var notificationName in notificationNames)
                {
                    changed |= subscribedNames.Remove(notificationName);
                }
            }

            if (subscribedNames.Count == 0)
            {
                next.Subscriptions.Remove(subscriberKey);
                changed = true;
            }
        }

        if (next.Pending.TryGetValue(subscriberKey, out var pending))
        {
            var remaining = next.Subscriptions.GetValueOrDefault(subscriberKey);
            if (pending.RemoveAll(p => remaining == null || !remaining.Contains(p.Name)) > 0)
            {
                changed = true;
            }

            if (pending.Count == 0)
            {
                next.Pending.Remove(subscriberKey);
            }
        }

        if (changed)
        {
            await CommitAsync(next);
        }
    }

    public async Task<NotificationDelivery[]> GetPendingDeliveriesAsync(string subscriberGrainKey, int maxCount)
    {
        maxCount = Math.Clamp(maxCount, 1, MaxPendingBatch);
        if (!State.Pending.TryGetValue(subscriberGrainKey, out var pending))
        {
            return [];
        }

        var nowUtc = DateTime.UtcNow;
        var subscribedNames = State.Subscriptions.GetValueOrDefault(subscriberGrainKey);
        var result = new List<NotificationDelivery>(Math.Min(maxCount, pending.Count));
        var invalid = new HashSet<Guid>();

        foreach (var delivery in pending)
        {
            var stored = FindBySequence(State.History, delivery.Sequence);
            if (stored == null || stored.IsExpired(nowUtc) || subscribedNames == null || !subscribedNames.Contains(delivery.Name))
            {
                invalid.Add(delivery.DeliveryId);
                continue;
            }

            result.Add(new NotificationDelivery
            {
                DeliveryId = delivery.DeliveryId,
                Notification = stored.Notification
            });

            if (result.Count == maxCount)
            {
                break;
            }
        }

        if (invalid.Count > 0)
        {
            try
            {
                await RemovePendingAsync(subscriberGrainKey, invalid);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to drop invalid pending deliveries of {Publisher} to {Subscriber}.",
                    this.GetPrimaryKeyString(), subscriberGrainKey);
            }
        }

        return result.ToArray();
    }

    public Task<bool> IsDeliveryPendingAsync(string subscriberGrainKey, Guid deliveryId)
    {
        if (!State.Pending.TryGetValue(subscriberGrainKey, out var pending) ||
            !State.Subscriptions.TryGetValue(subscriberGrainKey, out var subscribedNames))
        {
            return Task.FromResult(false);
        }

        var delivery = pending.Find(p => p.DeliveryId == deliveryId);
        return Task.FromResult(
            delivery != null &&
            subscribedNames.Contains(delivery.Name) &&
            FindBySequence(State.History, delivery.Sequence) is { } stored &&
            !stored.IsExpired(DateTime.UtcNow)
        );
    }

    public Task ConfirmDeliveriesAsync(string subscriberGrainKey, Guid[] deliveryIds)
        => deliveryIds is { Length: > 0 }
            ? RemovePendingAsync(subscriberGrainKey, deliveryIds.ToHashSet())
            : Task.CompletedTask;

    private async Task RemovePendingAsync(string subscriberGrainKey, HashSet<Guid> deliveryIds)
    {
        if (!State.Pending.TryGetValue(subscriberGrainKey, out var current) || !current.Any(p => deliveryIds.Contains(p.DeliveryId)))
        {
            return;
        }

        var next = State.Clone();
        var pending = next.Pending[subscriberGrainKey];
        pending.RemoveAll(p => deliveryIds.Contains(p.DeliveryId));
        if (pending.Count == 0)
        {
            next.Pending.Remove(subscriberGrainKey);
        }

        await CommitAsync(next);
    }

    private void KickAllPendingDeliveries()
    {
        foreach (var subscriberKey in State.Pending.Keys.ToArray())
        {
            KickDelivery(subscriberKey);
        }
    }

    /// <summary>
    /// Subscriber key -> whether another kick was requested while a signal to its worker is in flight.
    /// At most one outstanding signal per worker; further kicks are coalesced into a single follow-up.
    /// </summary>
    private readonly Dictionary<string, bool> kicksInFlight = new();

    private void KickDelivery(string subscriberKey)
    {
        if (kicksInFlight.ContainsKey(subscriberKey))
        {
            kicksInFlight[subscriberKey] = true;
            return;
        }

        kicksInFlight[subscriberKey] = false;
        _ = SignalWorkerAsync(subscriberKey);
    }

    private async Task SignalWorkerAsync(string subscriberKey)
    {
        try
        {
            var worker = grainFactory.GetGrain<INotificationDeliveryGrain>(
                StateflowsGrainKeys.ForDelivery(this.GetPrimaryKeyString(), subscriberKey));
            // Awaited only in this unawaited background task: DrainAsync completes when the worker's drain ends,
            // which keeps further kicks for this worker coalesced while it is processing
            await worker.DrainAsync();
        }
        catch (TimeoutException exception)
        {
            // Long drains exceed the Orleans response timeout; the worker keeps draining regardless
            logger.LogDebug(exception, "Notification delivery signal of {Publisher} to {Subscriber} timed out while worker is draining.",
                this.GetPrimaryKeyString(), subscriberKey);
        }
        catch (Exception exception)
        {
            StateflowsOrleansDiagnostics.PublicationFailed("signal");
            logger.LogWarning(exception, "Failed to signal notification delivery of {Publisher} to {Subscriber}; reminder will retry.",
                this.GetPrimaryKeyString(), subscriberKey);
        }
        finally
        {
            var again = kicksInFlight.Remove(subscriberKey, out var requested) && requested;
            if (again && State.Pending.ContainsKey(subscriberKey))
            {
                KickDelivery(subscriberKey);
            }
        }
    }

    #endregion

    #region Watches

    public Task<OrleansEventHolder[]> AddWatchAsync(IBehaviorGrainObserver observer, string[] notificationNames, DateTime? replayNotificationsSince = null, bool renewal = false)
    {
        ArgumentNullException.ThrowIfNull(observer);
        var names = ValidateNames(notificationNames);
        var nowUtc = DateTime.UtcNow;

        if (watches.TryGetValue(observer, out var entry) && entry.LastSeenUtc + WatchExpiration < nowUtc)
        {
            // Expired watch is reset so all requested names count as newly watched and get replayed;
            // notifications still queued for the old entry are discarded by the identity check on dispatch
            watches.Remove(observer);
            entry = null;
        }

        if (entry == null)
        {
            entry = new WatchEntry();
            watches.Add(observer, entry);
        }

        entry.LastSeenUtc = nowUtc;
        var justWatched = names.Where(entry.Names.Add).ToHashSet();
        var replayNames = renewal ? justWatched : names;

        var replay = Query(State, replayNames, replayNotificationsSince, nowUtc)
            .Select(s => s.Notification)
            .ToArray();

        if (replay.Length > 0)
        {
            EnqueueObserverNotification(observer, entry, replay);
        }

        return Task.FromResult(replay);
    }

    public Task RemoveWatchAsync(IBehaviorGrainObserver observer, string[]? notificationNames = null)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!watches.TryGetValue(observer, out var entry))
        {
            return Task.CompletedTask;
        }

        if (notificationNames == null)
        {
            entry.Names.Clear();
        }
        else
        {
            entry.Names.ExceptWith(notificationNames);
        }

        if (entry.Names.Count == 0)
        {
            watches.Remove(observer);
        }

        return Task.CompletedTask;
    }

    private void NotifyWatchers(List<StoredNotification> published, DateTime nowUtc)
    {
        if (published.Count == 0 || watches.Count == 0)
        {
            return;
        }

        foreach (var (observer, entry) in watches.ToArray())
        {
            if (entry.LastSeenUtc + WatchExpiration < nowUtc)
            {
                watches.Remove(observer);
                continue;
            }

            var filtered = published
                .Where(s => entry.Names.Contains(s.Notification.Name))
                .Select(s => s.Notification)
                .ToArray();

            if (filtered.Length > 0)
            {
                EnqueueObserverNotification(observer, entry, filtered);
            }
        }
    }

    private void EnqueueObserverNotification(IBehaviorGrainObserver observer, WatchEntry entry, OrleansEventHolder[] notifications)
        => entry.Tail = NotifyObserverAsync(entry.Tail, observer, entry, notifications);

    private async Task NotifyObserverAsync(Task previous, IBehaviorGrainObserver observer, WatchEntry entry, OrleansEventHolder[] notifications)
    {
        // Chained per observer to preserve publication order without awaiting observers in grain calls
        await previous;
        if (!watches.TryGetValue(observer, out var current) || !ReferenceEquals(current, entry))
        {
            return;
        }

        // Notifications may have expired while waiting for previous dispatches to this observer
        var nowUtc = DateTime.UtcNow;
        notifications = notifications
            .Where(n => !new StoredNotification { Notification = n }.IsExpired(nowUtc))
            .ToArray();
        if (notifications.Length == 0)
        {
            return;
        }

        try
        {
            await observer.NotifyAsync(notifications);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to notify watcher of {Publisher}; removing defunct watch.", this.GetPrimaryKeyString());
            if (watches.TryGetValue(observer, out current) && ReferenceEquals(current, entry))
            {
                watches.Remove(observer);
            }
        }
    }

    #endregion
}
