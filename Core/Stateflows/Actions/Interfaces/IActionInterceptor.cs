using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Actions
{
    public interface IActionInterceptor
    {
        Task AfterHydrateAsync(IActionDelegateContext context);

        Task BeforeDehydrateAsync(IActionDelegateContext context);

        Task<bool> BeforeProcessEventAsync<TEvent>(IEventContext<TEvent> context);

        Task AfterProcessEventAsync<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus);

    }
}
