using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common
{
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
        Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next);

        Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers);
        Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            where TRequest : IRequest<TResponse>;
    }
}
