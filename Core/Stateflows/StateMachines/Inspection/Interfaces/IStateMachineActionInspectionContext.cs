using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineActionInspectionContext : IBehaviorLocator
    {
        IStateMachineInspectionContext StateMachine { get; }
    }
}
