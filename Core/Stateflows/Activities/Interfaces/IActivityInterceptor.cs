using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityInterceptor
    {
        Task AfterHydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task BeforeDehydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Task.FromResult(true);

        Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context)
            => Task.CompletedTask;

    }
}
