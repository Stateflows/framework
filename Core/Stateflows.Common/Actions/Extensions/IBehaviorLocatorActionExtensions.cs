using Stateflows.Common;
using Stateflows.Common.Actions.Classes;

namespace Stateflows.Actions
{
    public static class IBehaviorLocatorActionExtensions
    {
        public static bool TryLocateAction(this IBehaviorLocator locator, ActionId id, out IActionBehavior action)
            => (
                action = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new ActionWrapper(behavior)
                    : null
            ) != null;
    }
}
