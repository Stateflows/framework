using Stateflows.Common;

namespace Stateflows.Actions
{
    /// <summary>Resolves client handles for action instances.</summary>
    public interface IActionLocator
    {
        /// <summary>Gets the underlying behavior locator used to resolve action handles.</summary>
        IBehaviorLocator Locator { get; }

        /// <summary>Attempts to resolve an action handle without executing or initializing the action.</summary>
        /// <param name="id">The action definition name and instance identifier.</param>
        /// <param name="activity">Receives the action handle when resolution succeeds, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when the behavior locator can resolve the action.</returns>
        bool TryLocateAction(ActionId id, out IActionBehavior activity);
    }
}
