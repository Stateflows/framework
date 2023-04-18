using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateActionInspectionContext : IStateActionContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
