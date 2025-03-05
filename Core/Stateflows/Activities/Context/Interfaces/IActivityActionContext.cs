using System;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityActionContext : IBehaviorLocator, IExecutionContext
    {
        /// <summary>
        /// Information about activity behavior
        /// </summary>
        [Obsolete("Activity context property is obsolete, use Behavior or LockHandle properties instead.")]
        IActivityContext Activity { get; }
        
        /// <summary>
        /// Behavior-level lock handle enabling developer to synchronize operations between Activity's nodes
        /// </summary>
        object LockHandle => Activity.LockHandle;

        /// <summary>
        /// Information about current behavior
        /// </summary>
        IBehaviorContext Behavior => Activity;
        
    }
}
