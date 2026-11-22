using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

/// <summary>Provides asynchronous event dispatch from a behavior context.</summary>
/// <typeparam name="TReturn">The context type used by related fluent API contracts.</typeparam>
public interface ISends<TReturn>
{
    /// <summary>
    /// Dispatches an event to the specified behavior context.
    /// </summary>
    /// <typeparam name="TEvent">The event payload type.</typeparam>
    /// <param name="event">The event payload to dispatch.</param>
    /// <param name="headers">Optional delivery headers.</param>
    Task<SendResult> DispatchAsync<TEvent>(TEvent @event, IDictionary<string, EventHeader> headers = null);
}