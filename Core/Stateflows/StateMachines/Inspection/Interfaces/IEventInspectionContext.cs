using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IEventInspectionContext<out TEvent> : IEventActionContext<TEvent>
    {
        [Obsolete("StateMachine context property is obsolete, use Behavior or CurrentState properties instead.")]
        new IStateMachineInspectionContext StateMachine { get; }
    }
}