namespace Stateflows.StateMachines
{
    /// <summary>Identifies the state whose deferral rule is being evaluated.</summary>
    public interface IDeferralContext
    {
        /// <summary>The state that may defer the current event for later processing.</summary>
        IStateContext State { get; }
    }
}
