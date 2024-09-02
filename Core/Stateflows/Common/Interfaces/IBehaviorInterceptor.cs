using System.Threading.Tasks;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Common
{
    public interface IBehaviorInterceptor
    {
        Task AfterHydrateAsync(IBehaviorActionContext context);
        Task BeforeDehydrateAsync(IBehaviorActionContext context);
        Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context);
        Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context);
    }
}
