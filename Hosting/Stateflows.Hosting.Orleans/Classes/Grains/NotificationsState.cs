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
    /// All stored notifications ordered by <see cref="StoredNotification.Sequence"/>.
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

    public NotificationsState Clone()
        => new()
        {
            Version = Version,
            LastSequence = LastSequence,
            // StoredNotification and PendingDelivery are treated as immutable, so shallow item copies are sufficient
            History = [..History],
            Subscriptions = Subscriptions.ToDictionary(p => p.Key, p => new HashSet<string>(p.Value)),
            Pending = Pending.ToDictionary(p => p.Key, p => new List<PendingDelivery>(p.Value)),
            LegacyMigrated = LegacyMigrated,
            LatestRetained = new Dictionary<string, long>(LatestRetained),
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
