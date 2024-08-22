using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IGuardInspectionContext<out TEvent> : ITransitionContext<TEvent>
        where TEvent : Event, new()
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
