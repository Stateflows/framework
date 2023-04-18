using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IGuardInspectionContext<out TEvent> : IGuardContext<TEvent>
        where TEvent : Event
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}
