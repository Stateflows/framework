using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineActionInspectionContext : IStateMachineActionContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
