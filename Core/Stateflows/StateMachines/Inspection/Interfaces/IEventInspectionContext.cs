using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IEventInspectionContext<out TEvent> : IEventActionContext<TEvent>
    {
        new IStateMachineInspectionContext StateMachine { get; }
    }
}