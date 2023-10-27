using Stateflows.Common;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityEventActionContext : IActivityActionContext
    {
        Event Event { get; }
    }
}
