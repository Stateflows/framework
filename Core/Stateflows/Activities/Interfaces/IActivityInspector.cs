using System;
using System.Threading.Tasks;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInspector
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationInspectionContext context);
        Task AfterActivityInitializeAsync(IActivityInitializationInspectionContext context);

        Task BeforeNodeInitializeAsync(IActivityNodeInspectionContext context);
        Task AfterNodeInitializeAsync(IActivityNodeInspectionContext context);

        Task BeforeNodeFinalizeAsync(IActivityNodeInspectionContext context);
        Task AfterNodeFinalizeAsync(IActivityNodeInspectionContext context);

        Task BeforeNodeExecuteAsync(IActivityNodeInspectionContext context);
        Task AfterNodeExecuteAsync(IActivityNodeInspectionContext context);

        Task BeforeFlowGuardAsync(IGuardInspectionContext context);
        Task AfterFlowGuardAsync(IGuardInspectionContext context, bool guardResult);

        Task BeforeFlowTransformationAsync(ITransformationInspectionContext context);
        Task AfterFlowTransformationAsync(ITransformationInspectionContext context);

        Task OnActivityInitializationExceptionAsync(IActivityInitializationInspectionContext context, Exception exception);
        Task OnNodeInitializationExceptionAsync(IActivityNodeInspectionContext context, Exception exception);
        Task OnNodeExecutionExceptionAsync(IActivityNodeInspectionContext context, Exception exception);
        Task OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception);
        Task OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception);
    }
}
