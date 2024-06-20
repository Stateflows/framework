using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        Task OnStateMachineInitializationExceptionAsync<TInitializationEvent>(IStateMachineInitializationContext<TInitializationEvent> context, Exception exception)
            where TInitializationEvent : Event, new()
            => Task.CompletedTask;

        Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.CompletedTask;

        Task OnTransitionGuardExceptionAsync(IGuardContext<Event> context, Exception exception)
            => Task.CompletedTask;

        Task OnTransitionEffectExceptionAsync(ITransitionContext<Event> context, Exception exception)
            => Task.CompletedTask;

        Task OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        Task OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;

        Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.CompletedTask;
    }
}
