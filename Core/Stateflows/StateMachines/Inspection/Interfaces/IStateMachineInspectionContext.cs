namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineInspectionContext : IStateMachineContext
    {
        IStateMachineInspection Inspection { get; }
    }
}
