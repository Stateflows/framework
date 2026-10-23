using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Entities
{
    /// <summary>
    /// Intercepts incoming entity events using an asynchronous continuation.
    /// </summary>
    public interface IEntityInterceptor
    {
        /// <summary>
        /// Wraps event processing and returns its resulting status.
        /// </summary>
        /// <remarks>
        /// Global interceptors wrap local interceptors in registration order.
        /// Cross-actor behavior interceptors wrap the entity interceptor pipeline.
        /// Call and await <paramref name="next"/> at most once. An interceptor may
        /// short-circuit processing by returning a status without calling it, or
        /// replace the returned status. Exceptions propagate unless explicitly handled.
        /// Repeated continuation calls are rejected. An unfinished continuation is
        /// awaited and reported as an invalid hook implementation before processing completes.
        /// </remarks>
        Task<EventStatus> ProcessEventAsync<TEvent>(
            IEventContext<TEvent> context,
            Func<Task<EventStatus>> next);
    }
}
