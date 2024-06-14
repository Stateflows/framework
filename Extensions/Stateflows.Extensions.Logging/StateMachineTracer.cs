using Stateflows.Common;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using System.Threading.Tasks;

namespace Stateflows.Extensions.Logging
{
    internal class StateMachineTracer : IStateMachineObserver
    {
        public Task AfterStateEntryAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateExitAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateFinalizeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateInitializeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterTransitionEffectAsync(ITransitionContext<Event> context)
        {
            return Task.CompletedTask;
        }

        public Task AfterTransitionGuardAsync(IGuardContext<Event> context, bool guardResult)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateEntryAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateExitAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateFinalizeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateInitializeAsync(IStateActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeTransitionEffectAsync(ITransitionContext<Event> context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeTransitionGuardAsync(IGuardContext<Event> context)
        {
            return Task.CompletedTask;
        }
    }
}
