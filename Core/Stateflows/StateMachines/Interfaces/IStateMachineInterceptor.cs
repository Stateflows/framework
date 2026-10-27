using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Provides middleware around state-machine event processing, with explicit short-circuit and status replacement support.</summary>
    /// <remarks>See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.</remarks>
    public interface IStateMachineInterceptor
    {
        /// <summary>
        /// Wraps event processing, including initialization. Globals wrap locals in registration order.
        /// </summary>
        /// <remarks>
        /// Call and await <paramref name="next"/> at most once, or return a status to short-circuit.
        /// The returned status may be replaced and continuation exceptions explicitly handled.
        /// Repeated or unfinished continuation calls are rejected.
        /// </remarks>
        /// <typeparam name="TEvent">The current event payload type.</typeparam>
        /// <param name="context">The current event envelope and machine execution context.</param>
        /// <param name="next">The next interceptor or engine operation; call and await at most once.</param>
        /// <returns>The event-processing status to report to the caller.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next);
    }
}
