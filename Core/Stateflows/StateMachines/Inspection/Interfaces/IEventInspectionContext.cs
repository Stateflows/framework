using System;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface IEventInspectionContext<out TEvent> : IEventContext<TEvent>
    {
        [Obsolete("StateMachine context property is obsolete, use Behavior or CurrentState properties instead.")]
        new IStateMachineInspectionContext StateMachine { get; }
    }
}