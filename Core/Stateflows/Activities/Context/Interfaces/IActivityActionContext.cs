using System;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityActionContext : IBehaviorActionContext
    {
        /// <summary>
        /// Information about activity behavior
        /// </summary>
        [Obsolete("Activity context property is obsolete, use Behavior, ActiveNodes, or LockHandle properties instead.")]
        IActivityContext Activity { get; }
        
        /// <summary>
        /// Behavior-level lock handle enabling developer to synchronize operations between Activity's nodes
        /// </summary>
        object LockHandle => Activity.LockHandle;

        /// <summary>
        /// Information about current behavior
        /// </summary>
        new IBehaviorContext Behavior => Activity;

        /// <summary>
        /// Tree of Nodes that represents current configuration of Activity behavior instance
        /// </summary>
        IReadOnlyTree<INodeContext> ActiveNodes => Activity.ActiveNodes;
    }
}
