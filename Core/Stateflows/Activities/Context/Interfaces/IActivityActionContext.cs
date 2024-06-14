using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityActionContext : IBehaviorLocator
    {
        /// <summary>
        /// Information about activity
        /// </summary>
        IActivityContext Activity { get; }
    }
}
