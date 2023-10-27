using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityActionContext : IBehaviorLocator
    {
        IActivityContext Activity { get; }
    }
}
