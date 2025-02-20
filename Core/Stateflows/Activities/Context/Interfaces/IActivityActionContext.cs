using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityActionContext : IBehaviorLocator, IExecutionContext
    {
        /// <summary>
        /// Information about activity behavior
        /// </summary>
        IActivityContext Activity { get; }
    }
}
