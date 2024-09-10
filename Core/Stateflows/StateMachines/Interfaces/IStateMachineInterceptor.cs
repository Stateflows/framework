using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);

        Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.CompletedTask;
    }
}
