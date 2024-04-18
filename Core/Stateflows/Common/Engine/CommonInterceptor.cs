using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Interfaces;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Engine
{
    internal class CommonInterceptor : IBehaviorInterceptor, IStateflowsExecutionInterceptor, IStateflowsTenantInterceptor
    {
        private readonly IEnumerable<IBehaviorInterceptor> Interceptors;

        private readonly IEnumerable<IStateflowsExecutionInterceptor> ExecutionInterceptors;

        private readonly IEnumerable<IStateflowsTenantInterceptor> TenantInterceptors;

        private readonly ILogger<CommonInterceptor> Logger;

        public CommonInterceptor(
            IEnumerable<IBehaviorInterceptor> interceptors,
            IEnumerable<IStateflowsExecutionInterceptor> executionInterceptors,
            IEnumerable<IStateflowsTenantInterceptor> tenantInterceptors,
            ILogger<CommonInterceptor> logger
        )
        {
            Interceptors = interceptors;
            ExecutionInterceptors = executionInterceptors;
            TenantInterceptors = tenantInterceptors;
            Logger = logger;
        }

        public Task AfterHydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);

        public Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);

        public Task BeforeDehydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);

        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

        public bool BeforeExecute(object @event)
        {
            var result = true;
            foreach (var interceptor in ExecutionInterceptors)
            {
                if (!interceptor.BeforeExecute(@event))
                {
                    result = false;
                }
            }

            return result;
        }

        public void AfterExecute(object @event)
        {
            foreach (var interceptor in ExecutionInterceptors)
            {
                interceptor.AfterExecute(@event);
            }
        }

        public bool BeforeExecute(string tenantId)
        {
            var result = true;
            foreach (var interceptor in TenantInterceptors)
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
            foreach (var interceptor in TenantInterceptors)
            {
                interceptor.AfterExecute(tenantId);
            }
        }
    }
}
