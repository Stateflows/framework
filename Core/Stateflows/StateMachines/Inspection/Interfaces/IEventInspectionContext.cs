using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IEventInspectionContext<out TEvent> : IEventContext<TEvent>
        where TEvent : Event
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
