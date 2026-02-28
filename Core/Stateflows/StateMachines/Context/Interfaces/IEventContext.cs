using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IStateMachineActionContext
    {
        TEvent Event { get; }

        Guid EventId { get; }
        
        IDictionary<string, EventHeader> EventHeaders { get; }
    }
}
