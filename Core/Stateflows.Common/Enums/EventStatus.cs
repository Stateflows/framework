namespace Stateflows.Common
{
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
        /// Target behavior not found
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
        /// Event was omitted because other events in CompoundRequest were invalid
        /// </summary>
        Omitted,
        /// <summary>
        /// Event was forwarded to embedded behavior
        /// </summary>
        Forwarded
    }
}
