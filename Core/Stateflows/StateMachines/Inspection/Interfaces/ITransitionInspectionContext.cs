using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface ITransitionInspectionContext<out TEvent> : ITransitionContext<TEvent>
        where TEvent : Event
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
