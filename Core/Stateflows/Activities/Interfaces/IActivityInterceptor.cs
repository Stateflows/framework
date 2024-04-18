using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInterceptor
    {
        Task AfterHydrateAsync(IActivityActionContext context);
        Task BeforeDehydrateAsync(IActivityActionContext context);
        Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context);
        Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context);
    }
}
