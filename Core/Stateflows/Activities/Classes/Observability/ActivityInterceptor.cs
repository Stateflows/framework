using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityInterceptor : IActivityInterceptor
    {
        public virtual Task AfterHydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeDehydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        public virtual Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Task.FromResult(true);

        public virtual Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Task.CompletedTask;

    }
}
