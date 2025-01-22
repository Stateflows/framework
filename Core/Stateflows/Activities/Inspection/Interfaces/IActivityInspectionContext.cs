namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityInspectionContext : IActivityContext
    {
        IActivityInspection Inspection { get; }
    }
}
