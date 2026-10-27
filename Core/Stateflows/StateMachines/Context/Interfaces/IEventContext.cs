using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    /// <summary>Provides the event being processed together with its envelope metadata and machine execution context.</summary>
    /// <typeparam name="TEvent">The payload type; named structural transitions use <c>StructuralEvent</c>.</typeparam>
    public interface IEventContext<out TEvent> : IStateMachineActionContext
    {
        /// <summary>The event payload supplied to the current operation.</summary>
        TEvent Event { get; }

        /// <summary>The unique identifier carried by the current event envelope.</summary>
        Guid EventId { get; }
        
        /// <summary>The headers associated with the current event envelope.</summary>
        IDictionary<string, EventHeader> EventHeaders { get; }
    }
}
