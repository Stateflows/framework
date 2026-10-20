namespace Stateflows;

/// <summary>
/// Persisted form of a behavior context. The context is stored as a Stateflows JSON payload because it contains
/// application-defined polymorphic values which cannot be resolved by the Orleans storage serializer type allow-list.
/// </summary>
[GenerateSerializer]
[Alias("Stateflows.BehaviorContextState")]
internal sealed class BehaviorContextState
{
    [Id(0)]
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Set when the record only keeps receipts/outbox infrastructure, not live behavior state.
    /// </summary>
    [Id(1)]
    public bool Tombstone { get; set; }

    [Id(2)]
    public List<OutboxPublication> Outbox { get; set; } = [];

    // Retained even after reset/drain: acceptance watermarks must never see a reused sequence.
    [Id(3)]
    public long LastPublicationSequence { get; set; }
}

[GenerateSerializer]
[Alias("Stateflows.OutboxPublication")]
internal sealed class OutboxPublication
{
    [Id(0)]
    public long Sequence { get; set; }

    [Id(1)]
    public string PublisherKey { get; set; } = string.Empty;

    [Id(2)]
    public OrleansEventHolder[] Notifications { get; set; } = [];
}
