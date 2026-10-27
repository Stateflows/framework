namespace Stateflows.Common
{
    /// <summary>Describes the lifecycle state of a behavior instance.</summary>
    public enum BehaviorStatus
    {
        /// <summary>The behavior status is not known.</summary>
        Unknown,
        /// <summary>The behavior has not been initialized.</summary>
        NotInitialized,
        /// <summary>The behavior has been initialized and can process events.</summary>
        Initialized,
        /// <summary>The behavior has been finalized.</summary>
        Finalized
    }
}
