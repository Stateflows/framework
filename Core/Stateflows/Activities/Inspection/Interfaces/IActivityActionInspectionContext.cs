using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityActionInspectionContext : IActivityInitializationContext
    {
        new IActivityInspectionContext Activity { get; }
    }
}
