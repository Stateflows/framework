using System;

namespace Stateflows.Common.Context.Interfaces
{
    /// <summary>Exposes an incoming event and its delivery identifier to an event hook.</summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    public interface IEventContext<out TEvent> : IBehaviorActionContext
    {
        /// <summary>Gets the incoming event payload.</summary>
        TEvent Event { get; }

        /// <summary>Gets the identifier of the event delivery.</summary>
        Guid EventId { get; }
    }
}
