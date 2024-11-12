using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        Task AfterHydrateAsync(IStateMachineActionContext context);

        Task BeforeDehydrateAsync(IStateMachineActionContext context);

        Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context);

        Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context);
    }
}
