using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal abstract class StateMachinePlugin : IStateMachinePlugin
    {
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();

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

        public virtual Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);
    }
}