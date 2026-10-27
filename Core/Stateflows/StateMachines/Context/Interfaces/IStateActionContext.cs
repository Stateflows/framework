namespace Stateflows.StateMachines
{
    /// <summary>Provides the executing state's context during entry, exit, initialization, or finalization.</summary>
    public interface IStateActionContext : IStateMachineActionContext
    {
        /// <summary>The lifecycle action currently executing.</summary>
        string ActionName { get; }

        /// <summary>The state whose lifecycle action is executing.</summary>
        IStateContext State { get; }
    }
}
