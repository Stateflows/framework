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

        Task<bool> BeforeProcessEventAsync(IEventActionContext<Event> context)
            => Task.FromResult(true);

        Task AfterProcessEventAsync(IEventActionContext<Event> context)
            => Task.CompletedTask;
    }
}
