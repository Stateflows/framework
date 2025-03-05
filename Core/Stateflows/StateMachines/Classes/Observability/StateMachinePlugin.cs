using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal abstract class StateMachinePlugin : IStateMachinePlugin
    {
        public virtual Task AfterHydrateAsync(IStateMachineActionContext context)
                => Task.CompletedTask;

        public virtual Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        [Obsolete]
        public virtual Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
            => Task.FromResult(true);

        [Obsolete]
        public virtual Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus)
            => Task.CompletedTask;

        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
            => next(context);

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