using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common
{
    public interface IBehaviorInterceptor
    {
        Task AfterHydrateAsync(IBehaviorActionContext context);
        Task BeforeDehydrateAsync(IBehaviorActionContext context);
        bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context);
        void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);
        Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification);
        Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            where TRequest : IRequest<TResponse>;
    }
}
