using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface ITransformationInspectionContext : ITransformationContext<TokenHolder>
    {
        IActivityInspection Inspection { get; }
    }
}
