namespace Stateflows;

/// <summary>
/// Builds and parses the string keys of Stateflows grains.
/// </summary>
/// <remarks>
/// Keys are segments joined by <c>/</c>, each escaped with <see cref="Uri.EscapeDataString(string)"/>, e.g.
/// <c>host/StateMachine/orders/42</c>. Escaping keeps keys unambiguous and free of characters with special meaning to
/// storage providers: Orleans ADO.NET storage formats grain keys with <see cref="string.Format(string, object[])"/>,
/// so keys must not contain <c>{</c> or <c>}</c>. A <see langword="null"/> tenant is encoded as <c>%</c>, which escaping
/// never produces on its own.
/// </remarks>
internal static class StateflowsGrainKeys
{
    private const char Separator = '/';
    private const char DeliverySeparator = '|';
    private const string NullSegment = "%";

    public static string ForBehavior(TenantBehaviorId id)
        => ForBehavior(id.TenantId, id.BehaviorId);

    public static string ForBehavior(string? tenantId, BehaviorId behaviorId)
        => Join(tenantId, behaviorId.Type, behaviorId.Name, behaviorId.Instance);

    public static TenantBehaviorId ParseBehavior(string key)
    {
        var segments = Split(key, 4);
        return new TenantBehaviorId
        {
            TenantId = segments[0]!,
            BehaviorId = new BehaviorId(Required(segments[1], key), Required(segments[2], key), segments[3]!)
        };
    }

    public static string ForBehaviorClass(string? tenantId, BehaviorClass behaviorClass)
        => Join(tenantId, behaviorClass.Type, behaviorClass.Name);

    public static TenantBehaviorClass ParseBehaviorClass(string key)
    {
        var segments = Split(key, 3);
        return new TenantBehaviorClass
        {
            TenantId = segments[0]!,
            BehaviorClass = new BehaviorClass(Required(segments[1], key), Required(segments[2], key))
        };
    }

    /// <summary>
    /// Key of a notification delivery worker; '|' is always escaped within behavior keys.
    /// </summary>
    public static string ForDelivery(string publisherGrainKey, string subscriberGrainKey)
        => $"{publisherGrainKey}{DeliverySeparator}{subscriberGrainKey}";

    public static (string PublisherGrainKey, string SubscriberGrainKey) ParseDelivery(string key)
    {
        var parts = key.Split(DeliverySeparator);
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            throw new ArgumentException($"Invalid notification delivery key '{key}'.", nameof(key));
        }

        return (parts[0], parts[1]);
    }

    private static string Join(string? tenantId, params string?[] segments)
        => string.Join(Separator, new[] { tenantId }.Concat(segments).Select(Escape));

    private static string Escape(string? segment)
        => segment == null ? NullSegment : Uri.EscapeDataString(segment);

    private static string?[] Split(string key, int count)
    {
        ArgumentNullException.ThrowIfNull(key);
        var segments = key.Split(Separator);
        if (segments.Length != count)
        {
            throw new ArgumentException($"Invalid Stateflows grain key '{key}': expected {count} segments.", nameof(key));
        }

        return segments.Select(segment => segment == NullSegment ? null : Uri.UnescapeDataString(segment)).ToArray();
    }

    private static string Required(string? segment, string key)
        => string.IsNullOrEmpty(segment)
            ? throw new ArgumentException($"Invalid Stateflows grain key '{key}': empty behavior type or name.", nameof(key))
            : segment;
}
