using Stateflows.Common;

namespace Stateflows.Actions
{
    public interface IActionLocator
    {
        IBehaviorLocator Locator { get; }

        bool TryLocateAction(ActionId id, out IActionBehavior activity);
    }
}
