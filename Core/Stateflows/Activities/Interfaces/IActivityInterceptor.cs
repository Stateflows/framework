using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInterceptor
    {
        Task AfterHydrateAsync(IActivityActionContext context);
        Task BeforeDehydrateAsync(IActivityActionContext context);
        Task<bool> BeforeProcessEventAsync(IEventContext<Event> context);
        Task AfterProcessEventAsync(IEventContext<Event> context);
    }
}
