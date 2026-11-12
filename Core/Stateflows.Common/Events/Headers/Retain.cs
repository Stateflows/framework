namespace Stateflows.Common
{
    /// <summary>Retains only the latest notification of its type for queries and subscription retries, until a positive TTL expires it.</summary>
    public sealed class Retain : EventHeader
    { }
}