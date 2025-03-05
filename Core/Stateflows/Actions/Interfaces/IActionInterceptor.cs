using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionInterceptor
    {
        Task AfterHydrateAsync(IActionDelegateContext context);

        Task BeforeDehydrateAsync(IActionDelegateContext context);

        Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context);

        Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context);

    }
}
