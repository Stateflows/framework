namespace Stateflows.Common
{
    /// <summary>Specifies which behavior state is preserved when a behavior is reset.</summary>
    public enum ResetMode
    {
        /// <summary>Preserves the behavior version and subscriptions while resetting runtime state.</summary>
        KeepVersionAndSubscriptions,
        /// <summary>Preserves subscriptions but resets the behavior version and runtime state.</summary>
        KeepSubscriptions,
        /// <summary>Resets runtime state, version, and subscriptions.</summary>
        Full
    }
}
