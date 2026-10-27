using Stateflows.Common;
using Stateflows.Common.Actions.Classes;

namespace Stateflows.Actions
{
    /// <summary>Adapts general behavior lookup to action-specific client handles.</summary>
    public static class IBehaviorLocatorActionExtensions
    {
        /// <summary>Attempts to locate a behavior and expose it as an action handle.</summary>
        /// <param name="locator">The behavior locator to query.</param>
        /// <param name="id">The action instance to locate.</param>
        /// <param name="iaiAgent">Receives the action handle on success, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when a behavior handle was resolved.</returns>
        public static bool TryLocateAction(this IBehaviorLocator locator, ActionId id, out IActionBehavior iaiAgent)
            => (
                iaiAgent = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new ActionWrapper(behavior)
                    : null
            ) != null;
    }
}
