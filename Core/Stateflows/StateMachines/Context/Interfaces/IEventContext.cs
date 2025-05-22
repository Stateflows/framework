using System;

namespace Stateflows.StateMachines
{
    public interface IEventContext<out TEvent> : IStateMachineActionContext
    {
        TEvent Event { get; }

        Guid EventId { get; }
    }
}
