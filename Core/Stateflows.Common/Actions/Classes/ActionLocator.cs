using Stateflows.Actions;

namespace Stateflows.Common.Actions.Classes
{
    internal class ActionLocator(IBehaviorLocator locator) : IActionLocator
    {
        public IBehaviorLocator Locator { get; } = locator;

        public bool TryLocateAction(ActionId id, out IActionBehavior activity)
            => Locator.TryLocateAction(id, out activity);
    }
}
