using Stateflows.Common;
using Stateflows.Common.Activities.Classes;

namespace Stateflows.Activities
{
    /// <summary>Adapts general behavior lookup to activity-specific client handles.</summary>
    public static class IBehaviorLocatorActivityExtensions
    {
        /// <summary>Attempts to locate a behavior and expose it as an activity handle.</summary>
        /// <param name="locator">The behavior locator to query.</param>
        /// <param name="id">The activity instance to locate.</param>
        /// <param name="activity">Receives the activity handle on success, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when a behavior handle was resolved.</returns>
        public static bool TryLocateActivity(this IBehaviorLocator locator, ActivityId id, out IActivityBehavior activity)
            => (
                activity = locator.TryLocateBehavior(id.BehaviorId, out var behavior)
                    ? new ActivityWrapper(behavior)
                    : null
            ) != null;
    }
}
