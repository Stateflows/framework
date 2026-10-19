namespace Stateflows.Common
{
    /// <summary>
    /// Disables deduplication by event id for a single event: a redelivery executes again and no receipt is recorded.
    /// Use only for idempotent events.
    /// </summary>
    public sealed class NoDeduplication : EventHeader
    { }
}
