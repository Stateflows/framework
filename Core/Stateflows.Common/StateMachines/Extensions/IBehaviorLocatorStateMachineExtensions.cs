using Stateflows.Common;
using Stateflows.Common.StateMachines.Classes;

namespace Stateflows.StateMachines
{
    /// <summary>Adapts general behavior lookup to state-machine client handles.</summary>
    public static class IBehaviorLocatorStateMachineExtensions
    {
        /// <summary>Attempts to locate a behavior and expose it as a state-machine handle.</summary>
        /// <param name="locator">The behavior locator to query.</param>
        /// <param name="id">The state-machine instance to locate.</param>
        /// <param name="stateMachine">Receives the handle on success, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when a behavior handle was resolved.</returns>
        public static bool TryLocateStateMachine(this IBehaviorLocator locator, StateMachineId id, out IStateMachineBehavior stateMachine)
        {
            stateMachine = locator.TryLocateBehavior(id, out var behavior)
                ? new StateMachineWrapper(behavior)
                : null;

            return stateMachine != null;
        }
    }
}
