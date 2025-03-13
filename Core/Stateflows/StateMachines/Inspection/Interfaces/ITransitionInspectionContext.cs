using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface ITransitionInspectionContext<out TEvent> : ITransitionContext<TEvent>

    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
