using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Inspection.Interfaces;

namespace Stateflows.Activities
{
    // public abstract class ActivityInspector : IActivityInspector
    // {
    //     public virtual Task BeforeActivityInitializeAsync(IActivityInitializationInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterActivityInitializeAsync(IActivityInitializationInspectionContext context, bool initialized)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeActivityFinalizeAsync(IActivityFinalizationInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterActivityFinalizeAsync(IActivityFinalizationInspectionContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeNodeInitializeAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterNodeInitializeAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeNodeFinalizeAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterNodeFinalizeAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeNodeActivateAsync(IActivityNodeContext context, bool activated)
    //         => Task.CompletedTask;
    //     public virtual Task AfterNodeActivateAsync(IActivityNodeContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeNodeExecuteAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterNodeExecuteAsync(IActivityNodeInspectionContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeFlowGuardAsync(IGuardInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterFlowGuardAsync(IGuardInspectionContext context, bool guardResult)
    //         => Task.CompletedTask;
    //
    //     public virtual Task BeforeFlowTransformAsync(ITransformationInspectionContext context)
    //         => Task.CompletedTask;
    //     public virtual Task AfterFlowTransformAsync(ITransformationInspectionContext context)
    //         => Task.CompletedTask;
    //
    //     public virtual Task OnActivityInitializationExceptionAsync(IActivityInitializationInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnActivityFinalizationExceptionAsync(IActivityFinalizationInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnNodeInitializationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnNodeFinalizationExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnNodeExecutionExceptionAsync(IActivityNodeInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnFlowGuardExceptionAsync(IGuardInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    //     public virtual Task OnFlowTransformationExceptionAsync(ITransformationInspectionContext context, Exception exception)
    //         => Task.CompletedTask;
    // }
}
