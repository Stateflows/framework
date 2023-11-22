using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IGuardInspectionContext : IGuardContext<Token>
    {
        IActivityInspection Inspection { get; }
    }
}
