using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityContext : IBehaviorContext
    {
        /// <summary>
        /// Identifier of current Activity behavior
        /// </summary>
        new ActivityId Id { get; }

        /// <summary>
        /// Behavior-level lock handle enabling developer to synchronize operations between Activity's nodes
        /// </summary>
        object LockHandle { get; }
    }
}
