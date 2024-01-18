using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface ITransformationInspectionContext : ITransformationContext<Token>
    {
        IActivityInspection Inspection { get; }
    }
}
