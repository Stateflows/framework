using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows;

/// <summary>
/// Durable state of a publisher's subscriptions grain: notification history, subscriptions and pending deliveries
/// are stored together so that publication is committed atomically.
/// </summary>
[GenerateSerializer]
[Alias("Stateflows.NotificationsState")]
internal sealed class NotificationsState
{
    public const int CurrentVersion = 1;

    [Id(0)]
    public int Version { get; set; } = CurrentVersion;

    [Id(1)]
    public long LastSequence { get; set; }

    /// <summary>
    /// Unexpired positive-TTL history and latest retained values, ordered by <see cref="StoredNotification.Sequence"/>.
    /// </summary>
    [Id(2)]
    public List<StoredNotification> History { get; set; } = [];

    /// <summary>
    /// Subscriber behavior grain key -> subscribed notification names.
    /// </summary>
    [Id(3)]
    public Dictionary<string, HashSet<string>> Subscriptions { get; set; } = [];

    /// <summary>
    /// Subscriber behavior grain key -> ordered pending deliveries.
    /// </summary>
    [Id(4)]
    public Dictionary<string, List<PendingDelivery>> Pending { get; set; } = [];

    /// <summary>
    /// Set when legacy retainedNotificationsState/subscriptionsState records were imported.
    /// </summary>
    [Id(5)]
    public bool LegacyMigrated { get; set; }

    /// <summary>
    /// Notification name -> sequence of the latest retained notification. Older retained notifications are never
    /// served as retained, even when the latest one has expired.
    /// </summary>
    [Id(6)]
    public Dictionary<string, long> LatestRetained { get; set; } = [];

    // One watermark per source behavior, independent of history TTL/pruning.
    [Id(7)]
    public Dictionary<string, long> PublicationWatermarks { get; set; } = [];

    [Flags]
    internal enum Collections
    {
        None = 0,
        History = 1,
        Subscriptions = 2,
        Pending = 4,
        LatestRetained = 8,
        PublicationWatermarks = 16,
    }

    // Unselected collections are shared and must not be mutated by the candidate.
    // A subscriber key limits nested set/list copying to that subscriber.
    public NotificationsState Clone(Collections collections, string? subscriberKey = null)
        => new()
        {
            Version = Version,
            LastSequence = LastSequence,
            // StoredNotification and PendingDelivery are treated as immutable, so shallow item copies are sufficient
            History = collections.HasFlag(Collections.History) ? [..History] : History,
            Subscriptions = collections.HasFlag(Collections.Subscriptions)
                ? Subscriptions.ToDictionary(p => p.Key,
                    p => subscriberKey == null || p.Key == subscriberKey ? new HashSet<string>(p.Value) : p.Value)
                : Subscriptions,
            Pending = collections.HasFlag(Collections.Pending)
                ? Pending.ToDictionary(p => p.Key,
                    p => subscriberKey == null || p.Key == subscriberKey ? new List<PendingDelivery>(p.Value) : p.Value)
                : Pending,
            LegacyMigrated = LegacyMigrated,
            LatestRetained = collections.HasFlag(Collections.LatestRetained)
                ? new Dictionary<string, long>(LatestRetained) : LatestRetained,
            PublicationWatermarks = collections.HasFlag(Collections.PublicationWatermarks)
                ? new Dictionary<string, long>(PublicationWatermarks) : PublicationWatermarks,
        };
}

[GenerateSerializer]
[Alias("Stateflows.StoredNotification")]
internal sealed class StoredNotification
{
    [Id(0)]
    public long Sequence { get; set; }

    [Id(1)]
    public OrleansEventHolder Notification { get; set; }

    public DateTime? ExpiresAtUtc
        => Notification.TimeToLive > 0
            ? Notification.SentAt.ToUniversalTime().AddSeconds(Notification.TimeToLive)
            : null;

    public bool IsExpired(DateTime nowUtc)
        => ExpiresAtUtc is { } expiresAt && expiresAt <= nowUtc;
}

[GenerateSerializer]
[Alias("Stateflows.PendingDelivery")]
internal sealed class PendingDelivery
{
    [Id(0)]
    public Guid DeliveryId { get; set; }

    /// <summary>
    /// Sequence of the referenced <see cref="StoredNotification"/>.
    /// </summary>
    [Id(1)]
    public long Sequence { get; set; }

    [Id(2)]
    public string Name { get; set; } = string.Empty;

    [Id(3)]
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Single delivery handed to a delivery worker; <see cref="Notification"/> keeps the original notification identifier.
/// </summary>
[GenerateSerializer]
[Alias("Stateflows.NotificationDelivery")]
internal struct NotificationDelivery
{
    [Id(0)]
    public Guid DeliveryId { get; set; }

    [Id(1)]
    public OrleansEventHolder Notification { get; set; }
}

internal static class NotificationDeliveryStatuses
{
    /// <summary>
    /// Statuses that complete a delivery; any other status (and any exception) is retried.
    /// </summary>
    public static bool IsAcknowledged(EventStatus status)
        => status is EventStatus.Initialized
            or EventStatus.Deferred
            or EventStatus.Consumed
            or EventStatus.Forwarded
            or EventStatus.NotConsumed
            or EventStatus.Invalid
            or EventStatus.Duplicated;
}
