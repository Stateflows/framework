using System.Threading.Tasks;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInspector
    {
        Task BeforeActivityInitializeAsync(IActivityActionInspectionContext context);
        Task AfterActivityInitializeAsync(IActivityActionInspectionContext context);

        //Task BeforeActivityFinalizeAsync(IActivityActionInspectionContext context);
        //Task AfterActivityFinalizeAsync(IActivityActionInspectionContext context);

        //Task BeforeStateInitializeAsync(IStateActionInspectionContext context);
        //Task AfterStateInitializeAsync(IStateActionInspectionContext context);
        //Task BeforeStateEntryAsync(IStateActionInspectionContext context);
        //Task AfterStateEntryAsync(IStateActionInspectionContext context);
        //Task BeforeStateExitAsync(IStateActionInspectionContext context);
        //Task AfterStateExitAsync(IStateActionInspectionContext context);
        //Task BeforeTransitionGuardAsync(IGuardInspectionContext<Event> context);
        //Task AfterTransitionGuardAsync(IGuardInspectionContext<Event> context, bool guardResult);
        //Task BeforeTransitionEffectAsync(ITransitionInspectionContext<Event> context);
        //Task AfterTransitionEffectAsync(ITransitionInspectionContext<Event> context);

        //Task OnActivityInitializeExceptionAsync(IActivityActionInspectionContext context, Exception exception);
        //Task OnTransitionGuardExceptionAsync(IGuardInspectionContext<Event> context, Exception exception);
        //Task OnTransitionEffectExceptionAsync(IEventInspectionContext<Event> context, Exception exception);
        //Task OnStateInitializeExceptionAsync(IStateActionInspectionContext context, Exception exception);
        //Task OnStateEntryExceptionAsync(IStateActionInspectionContext context, Exception exception);
        //Task OnStateExitExceptionAsync(IStateActionInspectionContext context, Exception exception);
    }
}
