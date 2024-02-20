using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityContext : IBehaviorContext
    {
        new ActivityId Id { get; }
    }
}
