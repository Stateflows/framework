using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityInitializationInspectionContext : IActivityInitializationContext
    {
        new IActivityInspectionContext Activity { get; }
    }
}
