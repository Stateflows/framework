using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityInspector
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationInspectionContext context)
            => Task.CompletedTask;
        Task AfterActivityInitializeAsync(IActivityInitializationInspectionContext context, bool initialized)
            => Task.CompletedTask;

        Task BeforeActivityFinalizeAsync(IActivityFinalizationInspectionContext context)
            => Task.CompletedTask;
        Task AfterActivityFinalizeAsync(IActivityFinalizationInspectionContext context)
            => Task.CompletedTask;

        Task BeforeNodeInitializeAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;
        Task AfterNodeInitializeAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;

        Task BeforeNodeFinalizeAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;
        Task AfterNodeFinalizeAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;

        Task BeforeNodeActivateAsync(IActivityNodeContext context, bool activated)
            => Task.CompletedTask;
        Task AfterNodeActivateAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task BeforeNodeExecuteAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;
        Task AfterNodeExecuteAsync(IActivityNodeInspectionContext context)
            => Task.CompletedTask;

        Task BeforeFlowGuardAsync(IGuardInspectionContext context)
            => Task.CompletedTask;
        Task AfterFlowGuardAsync(IGuardInspectionContext context, bool guardResult)
            => Task.CompletedTask;

        Task BeforeFlowTransformAsync(ITransformationInspectionContext context)
            => Task.CompletedTask;
        Task AfterFlowTransformAsync(ITransformationInspectionContext context)
            => Task.CompletedTask;

        Task OnActivityInitializationExceptionAsync(IActivityInitializationInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnActivityFinalizationExceptionAsync(IActivityFinalizationInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnNodeInitializationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnNodeFinalizationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnNodeExecutionExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception)
            => Task.CompletedTask;
        Task OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception)
            => Task.CompletedTask;
    }
}
