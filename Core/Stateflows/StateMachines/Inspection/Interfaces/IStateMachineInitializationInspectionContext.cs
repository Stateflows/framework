using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineInitializationInspectionContext : IStateMachineInitializationContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
