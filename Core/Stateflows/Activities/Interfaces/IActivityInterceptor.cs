using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>
    /// Wraps event processing for activities, including initialization, to implement
    /// cross-cutting behavior. For each event, global interceptors wrap local
    /// interceptors, with each group applied in registration order.
    /// </summary>
    /// <remarks>
    /// An interceptor may short-circuit by returning its own event status without
    /// invoking <paramref name="next"/>. Otherwise it must invoke and await
    /// <paramref name="next"/> exactly once before it completes. It may return a
    /// different status from the continuation's status, or handle an exception from
    /// the continuation; exceptions thrown by the interceptor itself propagate.
    /// Repeated invocations and completing before an invoked continuation completes
    /// are rejected.
    /// </remarks>
    public interface IActivityInterceptor
    {
        /// <summary>
        /// Intercepts processing of one event for an activity. This callback is
        /// entered before the remaining interceptor chain and activity event handling;
        /// the event context identifies both the event and the activity receiving it.
        /// </summary>
        /// <typeparam name="TEvent">The CLR type of the event being processed.</typeparam>
        /// <param name="eventContext">The event context, including the event and the activity's processing identity and state.</param>
        /// <param name="next">The continuation for remaining interceptors and activity event processing; invoke and await it once unless short-circuiting.</param>
        /// <returns>The status reported for event processing; this interceptor may return a different status from the continuation when it handles or short-circuits processing.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> eventContext, Func<Task<EventStatus>> next);
    }
}
