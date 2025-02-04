using Stateflows.Actions;

namespace Stateflows.Common.Actions.Classes
{
    internal class ActionLocator : IActionLocator
    {
        public IBehaviorLocator Locator { get; }

        public ActionLocator(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public bool TryLocateAction(ActionId id, out IActionBehavior activity)
            => Locator.TryLocateAction(id, out activity);
    }
}
