using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Provides middleware around state-machine event processing, including initialization.</summary>
    /// <remarks>
    /// Global interceptors wrap local interceptors, in registration order within each group.
    /// An interceptor may invoke <c>next</c> at most once, or omit it to short-circuit processing.
    /// When invoked, the continuation must be awaited before this method completes. The interceptor
    /// may return a different <see cref="EventStatus"/> than the continuation returned and may
    /// catch a continuation failure to handle it; failures it does not handle propagate. Repeated
    /// or unfinished continuation calls fail execution.
    /// See <see href="https://github.com/Stateflows/framework/wiki/4.5.-Observability">observability</see>.
    /// </remarks>
    public interface IStateMachineInterceptor
    {
        /// <summary>Intercepts processing of an event delivered to a state machine.</summary>
        /// <typeparam name="TEvent">The CLR payload type of the delivered event.</typeparam>
        /// <param name="eventContext">The event envelope and execution context for the target machine.</param>
        /// <param name="next">The next interceptor or machine operation; invoke and await it at most once, or omit it to short-circuit.</param>
        /// <returns>The status to report for event processing, which may be the continuation's status or a replacement status.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> eventContext, Func<Task<EventStatus>> next);
    }
}
