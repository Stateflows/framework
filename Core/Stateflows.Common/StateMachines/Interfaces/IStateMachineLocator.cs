namespace Stateflows.StateMachines
{
    /// <summary>Resolves client handles for state-machine instances.</summary>
    public interface IStateMachineLocator
    {
        /// <summary>Attempts to resolve a state-machine handle without initializing the state machine.</summary>
        /// <param name="id">The state-machine definition name and instance identifier.</param>
        /// <param name="stateMachine">Receives the handle on success, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when the behavior locator can resolve the state machine.</returns>
        public bool TryLocateStateMachine(StateMachineId id, out IStateMachineBehavior stateMachine);
    }
}
