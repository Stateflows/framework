using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Classes;

/// <summary>Provides no-op defaults for optional behavior interception callbacks.</summary>
/// <remarks>Override only the callbacks needed by an interceptor; the default event processor invokes the next handler directly.</remarks>
public abstract class BehaviorInterceptor : IBehaviorInterceptor
{
    /// <inheritdoc />
    public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        => next();

    /// <inheritdoc />
    public virtual Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
        => Task.CompletedTask;

    /// <inheritdoc />
    public virtual Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response) where TRequest : IRequest<TResponse>
        => Task.CompletedTask;
}