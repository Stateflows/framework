using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

public interface ISends<TReturn>
{
    /// <summary>
    /// Sends an event to behavior
    /// </summary>
    /// <typeparam name="TEvent">Type of event</typeparam>
    /// <param name="event">Event instance</param>
    /// <param name="headers">Event headers</param>
    void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null);
}