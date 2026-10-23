using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Classes;

public abstract class BehaviorInterceptor : IBehaviorInterceptor
{
    public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        => next();

    public virtual Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification, IDictionary<string, EventHeader> headers)
        => Task.CompletedTask;

    public virtual Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response) where TRequest : IRequest<TResponse>
        => Task.CompletedTask;
}