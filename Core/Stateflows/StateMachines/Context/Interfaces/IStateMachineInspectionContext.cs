using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IStateMachineInspectionContext : IStateMachineContext
    {
        IStateMachineInspection Inspection { get; }
    }
}
