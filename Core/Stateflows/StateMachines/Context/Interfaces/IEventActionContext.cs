using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent>
    {
        TEvent Event { get; }

        Guid EventId { get; }

        IEnumerable<EventHeader> Headers { get; }
    }

    public interface IEventActionContext<out TEvent> : IEventContext<TEvent>, IStateMachineActionContext
    { }
}
