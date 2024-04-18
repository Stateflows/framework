using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception);
        Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception);
        Task OnTransitionGuardExceptionAsync<TEvent>(IGuardContext<TEvent> context, Exception exception);
        Task OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception);
        Task OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception);
        Task OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception);
        Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception);
        Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception);
    }
}
