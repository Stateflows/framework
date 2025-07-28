using System;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IStateMachineActionContext
    {
        TEvent Event { get; }

        Guid EventId { get; }
    }
}
