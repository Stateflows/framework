using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface IStateActionInspectionContext : IStateActionContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
