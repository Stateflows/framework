using Stateflows.Common;
using Stateflows.Common.Activities.Classes;

namespace Stateflows.Activities
{
    public static class IBehaviorLocatorActivityExtensions
    {
        public static bool TryLocateActivity(this IBehaviorLocator locator, ActivityId id, out IActivity activity)
            => (
                activity = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new ActivityWrapper(behavior)
                    : null
            ) != null;
    }
}
