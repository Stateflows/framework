using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common
{
    /// <summary>Observes and controls behavior event processing, notification publication, and request responses.</summary>
    public interface IBehaviorInterceptor
    {
        /// <summary>
        /// Wraps event processing for every actor type, outside actor-specific interceptors.
        /// </summary>
        /// <remarks>
        /// Interceptors run in registration order. Call and await <paramref name="next"/> at most once,
        /// or return a status without calling it. Status replacement and explicit exception handling
        /// are allowed. Repeated or unfinished continuation calls are rejected.
        /// </remarks>
        /// <typeparam name="TEvent">The event payload type being processed.</typeparam>
        /// <param name="context">The event and behavior execution context.</param>
        /// <param name="next">The continuation that processes the event.</param>
        /// <returns>The resulting event status.</returns>
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next);

        /// <summary>Observes publication of a notification by a behavior.</summary>
        /// <typeparam name="TNotification">The notification payload type.</typeparam>
        /// <param name="context">The behavior action context for the publication.</param>
        /// <param name="notification">The published payload.</param>
        /// <param name="headers">The event headers applied to the notification.</param>
        /// <returns>A task that completes when observation has finished.</returns>
        Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers);
        /// <summary>Observes completion of a request-response exchange.</summary>
        /// <typeparam name="TRequest">The request payload type.</typeparam>
        /// <typeparam name="TResponse">The response payload type.</typeparam>
        /// <param name="context">The behavior action context for the response.</param>
        /// <param name="request">The original request payload.</param>
        /// <param name="response">The response payload.</param>
        /// <returns>A task that completes when observation has finished.</returns>
        Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            where TRequest : IRequest<TResponse>;
    }
}
