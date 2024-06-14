using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
            => Task.FromResult(true);

        Task AfterProcessEventAsync(IEventContext<Event> context)
            => Task.CompletedTask;
    }
}
