using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common.Extensions;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common.Engine
{
    internal class CommonInterceptor : IBehaviorInterceptor, IExecutionInterceptor
    {
        public CommonInterceptor(
            IEnumerable<IBehaviorInterceptor> interceptors,
            IEnumerable<IExecutionInterceptor> executionInterceptors
        )
        {
            Interceptors = interceptors;
            ExecutionInterceptors = executionInterceptors;
        }

        private IEnumerable<IBehaviorInterceptor> Interceptors { get; }

        private IEnumerable<IExecutionInterceptor> ExecutionInterceptors { get; }

        public Task AfterHydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.AfterHydrateAsync(context), nameof(AfterHydrateAsync));

        public Task AfterProcessEventAsync(IEventContext<Event> context)
            => Interceptors.RunSafe(i => i.AfterProcessEventAsync(context), nameof(AfterProcessEventAsync));

        public Task BeforeDehydrateAsync(IBehaviorActionContext context)
            => Interceptors.RunSafe(i => i.BeforeDehydrateAsync(context), nameof(BeforeDehydrateAsync));

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Interceptors.RunSafe(i => i.BeforeProcessEventAsync(context), nameof(BeforeProcessEventAsync));

        public bool BeforeExecute(Event @event)
        {
            foreach (var interceptor in ExecutionInterceptors)
            {
                if (!interceptor.BeforeExecute(@event))
                {
                    return false;
                }
            }

            return true;
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
