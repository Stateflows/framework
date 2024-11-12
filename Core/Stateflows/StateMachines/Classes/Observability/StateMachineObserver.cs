using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineObserver : IStateMachineObserver
    {
        public virtual Task BeforeStateMachineInitializeAsync(IStateMachineInitializationContext context)
                => Task.CompletedTask;

        public virtual Task AfterStateMachineInitializeAsync(IStateMachineInitializationContext context, bool initialized)
            => Task.CompletedTask;

        public virtual Task BeforeStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public virtual Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task AfterStateInitializeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task AfterStateFinalizeAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task AfterStateEntryAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task AfterStateExitAsync(IStateActionContext context)
            => Task.CompletedTask;

        public virtual Task BeforeTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;

        public virtual Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
            => Task.CompletedTask;

        public virtual Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;

        public virtual Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
            => Task.CompletedTask;
    }
}