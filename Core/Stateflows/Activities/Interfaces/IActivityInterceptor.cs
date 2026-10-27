using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Intercepts activity event processing to implement cross-cutting behavior.</summary>
    /// <remarks>Interceptors wrap event processing, including initialization; global interceptors wrap local ones in registration order.</remarks>
    public interface IActivityInterceptor
    {
        /// <summary>
        /// Wraps event processing, including initialization. Globals wrap locals in registration order.
        /// </summary>
        /// <remarks>
        /// Call and await <paramref name="next"/> at most once, or return a status to short-circuit.
        /// The returned status may be replaced and continuation exceptions explicitly handled.
        /// Repeated or unfinished continuation calls are rejected.
        /// </remarks>
        /// <typeparam name="TEvent">The event type being processed.</typeparam>
        /// <param name="context">The event and activity context.</param>
        /// <param name="next">The continuation representing the remaining pipeline and event processing.</param>
        /// <returns>The event processing status, which an interceptor may replace when short-circuiting.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next);
    }
}
