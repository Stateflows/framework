using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Resolves client handles for activity instances.</summary>
    public interface IActivityLocator
    {
        /// <summary>Gets the underlying behavior locator used to resolve activity handles.</summary>
        IBehaviorLocator Locator { get; }

        /// <summary>Attempts to resolve an activity handle without executing or initializing the activity.</summary>
        /// <param name="id">The activity definition name and instance identifier.</param>
        /// <param name="activity">Receives the activity handle on success, or <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> when the behavior locator can resolve the activity.</returns>
        bool TryLocateActivity(ActivityId id, out IActivityBehavior activity);
    }
}
