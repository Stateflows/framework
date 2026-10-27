namespace Stateflows.Common
{
    /// <summary>Describes the outcome of processing an event for a behavior.</summary>
    public enum EventStatus
    {
        /// <summary>
        /// Event has initialized behavior instance
        /// </summary>
        Initialized,
        /// <summary>
        /// Event has failed to initialize behavior instance
        /// </summary>
        NotInitialized,
        /// <summary>
        /// Event has executed behavior instance
        /// </summary>
        Undelivered,
        /// <summary>
        /// Behavior is not yet initialized or already finalized and rejects all incoming events
        /// </summary>
        Rejected,
        /// <summary>
        /// Event was invalid, detailed informations can be found in Validation field
        /// </summary>
        Invalid,
        /// <summary>
        /// Event was deferred by behavior and can be consumed later
        /// </summary>
        Deferred,
        /// <summary>
        /// Event was consumed and processed by behavior
        /// </summary>
        Consumed,
        /// <summary>
        /// Event was not consumed by behavior
        /// </summary>
        NotConsumed,
        /// <summary>
        /// Event caused unhandled exception in behavior
        /// </summary>
        Failed,
        /// <summary>
        /// Event was forwarded to embedded behavior
        /// </summary>
        Forwarded,
        /// <summary>
        /// Event processing was cancelled
        /// </summary>
        Cancelled,
        /// <summary>
        /// Event was already processed by this behavior within its deduplication window
        /// </summary>
        Duplicated,
        /// <summary>
        /// Event was not executed because the behavior's resource is overloaded or its host is shutting down; it can be retried
        /// </summary>
        Overloaded
    }
}
