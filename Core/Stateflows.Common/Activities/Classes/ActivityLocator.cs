using Stateflows.Common.Interfaces;
using Stateflows.Activities;

namespace Stateflows.Common.Activities.Classes
{
    internal class ActivityLocator : IActivityLocator
    {
        public IBehaviorLocator Locator { get; }

        public ActivityLocator(IBehaviorLocator locator)
        {
            Locator = locator;
        }

        public bool TryLocateActivity(ActivityId id, out IActivity activity)
            => Locator.TryLocateActivity(id, out activity);
    }
}
