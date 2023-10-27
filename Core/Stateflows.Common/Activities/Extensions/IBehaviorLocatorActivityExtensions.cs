using Stateflows.Common.Interfaces;
using Stateflows.Common.Activities.Classes;

namespace Stateflows.Activities
{
    public static class IBehaviorLocatorActivityExtensions
    {
        public static bool TryLocateActivity(this IBehaviorLocator locator, ActivityId id, out IActivity activity)
#pragma warning disable S1121 // Assignments should not be made from within sub-expressions
            => (
                activity = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new ActivityWrapper(behavior)
                    : null
            ) != null;
#pragma warning restore S1121 // Assignments should not be made from within sub-expressions
    }
}
