using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Engine
{
    internal class CommonInterceptor(
        IEnumerable<IBehaviorInterceptor> interceptors,
        IEnumerable<IStateflowsExecutionInterceptor> executionInterceptors,
        IEnumerable<IStateflowsTenantInterceptor> tenantInterceptors,
        ILogger<CommonInterceptor> logger)
    {
        public void AfterHydrate(IBehaviorActionContext context)
            => interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrate), logger);

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
            => interceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), logger);

        public void BeforeDehydrate(IBehaviorActionContext context)
            => interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrate), logger);

        public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), logger);

        public bool BeforeExecute(BehaviorId id, EventHolder eventHolder)
        {
            var result = true;
            foreach (var interceptor in executionInterceptors)
            {
                if (!interceptor.BeforeExecute(id, eventHolder))
                {
                    result = false;
                }
            }

            return result;
        }

        public void AfterExecute(BehaviorId id, EventHolder eventHolder)
        {
            foreach (var interceptor in executionInterceptors.Reverse())
            {
                interceptor.AfterExecute(id, eventHolder);
            }
        }

        public bool BeforeExecute(string tenantId)
        {
            var result = true;
            foreach (var interceptor in tenantInterceptors)
            {
                if (!interceptor.BeforeExecute(tenantId))
                {
                    result = false;
                }
            }

            return result;
        }

        public void AfterExecute(string tenantId)
        {
            foreach (var interceptor in tenantInterceptors.Reverse())
            {
                interceptor.AfterExecute(tenantId);
            }
        }

        public async Task NotificationPublishedAsync<TNotification>(IBehaviorActionContext context, TNotification notification)
        {
            foreach (var interceptor in interceptors)
            {
                await interceptor.NotificationPublishedAsync(context, notification);
            }
        }

        public async Task RequestRespondedAsync<TRequest, TResponse>(IBehaviorActionContext context, TRequest request, TResponse response)
            where TRequest : IRequest<TResponse>
        {
            foreach (var interceptor in interceptors)
            {
                await interceptor.RequestRespondedAsync(context, request, response);
            }
        }

        private static readonly MethodInfo RequestRespondedAsyncMethod =
            typeof(CommonInterceptor).GetMethod(nameof(RequestRespondedAsync));
        
        public void RequestResponded(IBehaviorActionContext context, EventHolder request, EventHolder response)
            => ((Task)RequestRespondedAsyncMethod
                .MakeGenericMethod(request.PayloadType, response.PayloadType)
                .Invoke(this, [context, request.BoxedPayload, response.BoxedPayload])!).Wait();
    }
}