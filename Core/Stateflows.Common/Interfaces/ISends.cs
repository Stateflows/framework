using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides fire-and-forget event sending from a behavior context.</summary>
/// <typeparam name="TReturn">The context type used by related fluent API contracts.</typeparam>
public interface ISends<TReturn>
{
    /// <summary>
    /// Sends an event to the specified behavior context.
    /// </summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    /// <param name="event">The event payload to send.</param>
    /// <param name="headers">Optional delivery headers.</param>
    void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null);
}