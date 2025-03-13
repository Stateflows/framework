using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface IStateMachineActionInspectionContext : IStateMachineActionContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
