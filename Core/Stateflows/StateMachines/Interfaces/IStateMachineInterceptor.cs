using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInterceptor
    {
        Task AfterHydrateAsync(IStateMachineActionContext context);
        Task BeforeDehydrateAsync(IStateMachineActionContext context);
        Task<bool> BeforeProcessEventAsync(IEventContext<Event> context);
        Task AfterProcessEventAsync(IEventContext<Event> context);
    }
}
