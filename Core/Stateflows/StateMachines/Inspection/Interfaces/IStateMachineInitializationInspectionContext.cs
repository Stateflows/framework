using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface IStateMachineInitializationInspectionContext : IStateMachineInitializationContext
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
