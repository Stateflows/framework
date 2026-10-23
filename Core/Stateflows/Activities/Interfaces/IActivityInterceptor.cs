using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
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
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next);
    }
}
