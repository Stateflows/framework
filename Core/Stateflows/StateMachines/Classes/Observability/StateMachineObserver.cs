using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class StateMachineObserver : IStateMachineObserver
    {
        public virtual Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        public virtual Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
            => next();

        public virtual Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
            => next();

        public virtual Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
            => next();

        public virtual Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
            => next();

        public virtual Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
            => next();

        public virtual Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
            => next();

        public virtual Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
            => next();
    }
}