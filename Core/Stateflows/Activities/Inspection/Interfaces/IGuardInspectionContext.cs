using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IGuardInspectionContext : IGuardContext<Token>
    {
        IActivityInspection Inspection { get; }
    }
}
