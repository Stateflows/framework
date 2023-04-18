using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineExceptionHandler
    {
        Task OnStateMachineInitializeExceptionAsync(IStateMachineActionContext context, Exception exception);
        Task OnTransitionGuardExceptionAsync(IGuardContext<Event> context, Exception exception);
        Task OnTransitionEffectExceptionAsync(ITransitionContext<Event> context, Exception exception);
        Task OnStateInitializeExceptionAsync(IStateActionContext context, Exception exception);
        Task OnStateEntryExceptionAsync(IStateActionContext context, Exception exception);
        Task OnStateExitExceptionAsync(IStateActionContext context, Exception exception);
    }
}
