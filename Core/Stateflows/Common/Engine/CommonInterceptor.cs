using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Engine
{
    internal class CommonInterceptor
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

        public void AfterHydrate(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrate), Logger);

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
            => Interceptors.RunSafe(i => i.AfterProcessEvent(context, eventStatus), nameof(AfterProcessEvent), Logger);

        public void BeforeDehydrate(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrate), Logger);

        public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
            => Interceptors.RunSafe(i => i.BeforeProcessEvent(context), nameof(BeforeProcessEvent), Logger);

        public bool BeforeExecute(EventHolder eventHolder)
        {
            var result = true;
            foreach (var interceptor in ExecutionInterceptors)
            {
                if (!interceptor.BeforeExecute(eventHolder))
                {
                    result = false;
                }
            }

            return result;
        }

        public void AfterExecute(EventHolder eventHolder)
        {
            foreach (var interceptor in ExecutionInterceptors.Reverse())
            {
                interceptor.AfterExecute(eventHolder);
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
            foreach (var interceptor in TenantInterceptors.Reverse())
            {
                interceptor.AfterExecute(tenantId);
            }
        }
    }
}
