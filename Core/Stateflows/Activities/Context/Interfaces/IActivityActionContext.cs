using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityActionContext : IBehaviorLocator
    {
        IActivityContext Activity { get; }
    }
}
