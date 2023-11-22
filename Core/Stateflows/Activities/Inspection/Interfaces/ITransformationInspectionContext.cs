using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface ITransformationInspectionContext : ITransformationContext<Token>
    {
        IActivityInspection Inspection { get; }
    }
}
