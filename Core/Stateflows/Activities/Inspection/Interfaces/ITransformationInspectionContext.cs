using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface ITransformationInspectionContext : ITransformationContext<TokenHolder>
    {
        IActivityInspection Inspection { get; }
    }
}
