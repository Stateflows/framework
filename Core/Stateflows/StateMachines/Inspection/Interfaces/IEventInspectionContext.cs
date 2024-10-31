using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IEventInspectionContext<out TEvent> : IEventActionContext<TEvent>
        where TEvent : Event, new()
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}