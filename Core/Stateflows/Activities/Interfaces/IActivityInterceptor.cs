using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInterceptor
    {
        Task AfterHydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task BeforeDehydrateAsync(IActivityActionContext context)
            => Task.CompletedTask;

        Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);

        Task AfterProcessEventAsync(IEventContext<Event> context)
            => Task.CompletedTask;

    }
}
