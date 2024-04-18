using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines
{
    public interface IStateMachineInspector
    {
        Task BeforeStateMachineInitializeAsync(IStateMachineInitializationInspectionContext context);
        Task AfterStateMachineInitializeAsync(IStateMachineInitializationInspectionContext context);
        Task BeforeStateMachineFinalizeAsync(IStateMachineActionInspectionContext context);
        Task AfterStateMachineFinalizeAsync(IStateMachineActionInspectionContext context);
        Task BeforeStateInitializeAsync(IStateActionInspectionContext context);
        Task AfterStateInitializeAsync(IStateActionInspectionContext context);
        Task BeforeStateFinalizeAsync(IStateActionInspectionContext context);
        Task AfterStateFinalizeAsync(IStateActionInspectionContext context);
        Task BeforeStateEntryAsync(IStateActionInspectionContext context);
        Task AfterStateEntryAsync(IStateActionInspectionContext context);
        Task BeforeStateExitAsync(IStateActionInspectionContext context);
        Task AfterStateExitAsync(IStateActionInspectionContext context);
        Task BeforeTransitionGuardAsync<TEvent>(IGuardInspectionContext<TEvent> context);
        Task AfterTransitionGuardAsync<TEvent>(IGuardInspectionContext<TEvent> context, bool guardResult);
        Task BeforeTransitionEffectAsync<TEvent>(ITransitionInspectionContext<TEvent> context);
        Task AfterTransitionEffectAsync<TEvent>(ITransitionInspectionContext<TEvent> context);

        Task OnStateMachineInitializationExceptionAsync(IStateMachineInitializationInspectionContext context, Exception exception);
        Task OnStateMachineFinalizationExceptionAsync(IStateMachineActionInspectionContext context, Exception exception);
        Task OnTransitionGuardExceptionAsync<TEvent>(IGuardInspectionContext<TEvent> context, Exception exception);
        Task OnTransitionEffectExceptionAsync<TEvent>(IEventInspectionContext<TEvent> context, Exception exception);
        Task OnStateInitializeExceptionAsync(IStateActionInspectionContext context, Exception exception);
        Task OnStateFinalizeExceptionAsync(IStateActionInspectionContext context, Exception exception);
        Task OnStateEntryExceptionAsync(IStateActionInspectionContext context, Exception exception);
        Task OnStateExitExceptionAsync(IStateActionInspectionContext context, Exception exception);
    }
}
