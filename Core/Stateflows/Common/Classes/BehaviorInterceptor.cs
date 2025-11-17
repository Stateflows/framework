using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Classes;

public abstract class BehaviorInterceptor : IBehaviorInterceptor
{
    public virtual Task AfterHydrateAsync(IBehaviorActionContext context)
            => Task.CompletedTask;

    public virtual Task BeforeDehydrateAsync(IBehaviorActionContext context)
        => Task.CompletedTask;

    public virtual bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        => true;

    public virtual void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
    { }

    public virtual Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification)
        => Task.CompletedTask;

    public virtual Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response) where TRequest : IRequest<TResponse>
        => Task.CompletedTask;
}