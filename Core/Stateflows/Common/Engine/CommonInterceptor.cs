using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Interfaces;
using Microsoft.Extensions.Logging;

namespace Stateflows.Common.Engine
{
    internal class CommonInterceptor : IBehaviorInterceptor, IStateflowsExecutionInterceptor
    {
        private readonly IEnumerable<IBehaviorInterceptor> Interceptors;

        private readonly IEnumerable<IStateflowsExecutionInterceptor> ExecutionInterceptors;

        private readonly ILogger<CommonInterceptor> Logger;

        public CommonInterceptor(
            IEnumerable<IBehaviorInterceptor> interceptors,
            IEnumerable<IStateflowsExecutionInterceptor> executionInterceptors,
            ILogger<CommonInterceptor> logger
        )
        {
            Interceptors = interceptors;
            ExecutionInterceptors = executionInterceptors;
            Logger = logger;
        }

        public Task AfterHydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync), Logger);

        public Task AfterProcessEventAsync(IEventContext<Event> context)
            => Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync), Logger);

        public Task BeforeDehydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync), Logger);

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync), Logger);

        public bool BeforeExecute(Event @event)
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

        public void AfterExecute(Event @event)
        {
            foreach (var interceptor in ExecutionInterceptors)
            {
                interceptor.AfterExecute(@event);
            }
        }
    }
}
