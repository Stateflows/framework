using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IBehaviorLocator
        where TEvent : Event
    {
        TEvent Event { get; }

        IStateMachineContext StateMachine { get; }
    }
}
