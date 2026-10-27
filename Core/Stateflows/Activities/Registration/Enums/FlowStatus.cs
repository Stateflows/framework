namespace Stateflows.Activities
{
    /// <summary>Describes the result of evaluating an activity flow.</summary>
    public enum FlowStatus
    {
        /// <summary>The flow was eligible and activated.</summary>
        Activated,
        /// <summary>The flow was evaluated but did not activate.</summary>
        NotActivated,
        /// <summary>The flow was not evaluated in the current operation.</summary>
        NotUsed,
    }
}
