using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityNodeInspectionContext : IActivityNodeContext
    {
        new IActivityInspectionContext Activity { get; }
    }
}
