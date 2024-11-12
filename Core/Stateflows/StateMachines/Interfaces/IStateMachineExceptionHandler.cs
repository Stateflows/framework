using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception);

        Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception);

        Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception);

        Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception);

        Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception);

        Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception);
    }
}
