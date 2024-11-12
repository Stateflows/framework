using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineInterceptor : IStateMachineInterceptor
    {
        public virtual Task AfterHydrateAsync(IStateMachineActionContext context)
                => Task.CompletedTask;

        public virtual Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public virtual Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);

        public virtual Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.CompletedTask;
    }
}