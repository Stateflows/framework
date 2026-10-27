namespace Stateflows.Activities
{
    /// <summary>Describes the runtime status of an activity node.</summary>
    public enum NodeStatus
    {
        /// <summary>The node action executed.</summary>
        Executed,
        /// <summary>The node was skipped by the current execution path.</summary>
        Omitted,
        /// <summary>The node is currently active.</summary>
        Active,
        /// <summary>The node failed during execution.</summary>
        Failed,
        /// <summary>The node was not used in the current operation.</summary>
        NotUsed,
    }
}
