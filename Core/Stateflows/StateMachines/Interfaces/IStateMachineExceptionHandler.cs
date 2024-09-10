using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
            => Task.FromResult(false);
    }
}
