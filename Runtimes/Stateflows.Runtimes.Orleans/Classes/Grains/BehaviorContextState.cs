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
    /// Set when the record only keeps receipts, not live behavior state.
    /// </summary>
    [Id(1)]
    public bool Tombstone { get; set; }
}
