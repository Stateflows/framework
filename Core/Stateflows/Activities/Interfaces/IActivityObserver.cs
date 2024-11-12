using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationContext context);
        Task AfterActivityInitializeAsync(IActivityInitializationContext context, bool initialized);

        Task BeforeActivityFinalizeAsync(IActivityFinalizationContext context);
        Task AfterActivityFinalizeAsync(IActivityFinalizationContext context);

        Task BeforeNodeInitializeAsync(IActivityNodeContext context);
        Task AfterNodeInitializeAsync(IActivityNodeContext context);

        Task BeforeNodeFinalizeAsync(IActivityNodeContext context);
        Task AfterNodeFinalizeAsync(IActivityNodeContext context);

        Task BeforeNodeActivateAsync(IActivityNodeContext context, bool activated);
        Task AfterNodeActivateAsync(IActivityNodeContext context);

        Task BeforeNodeExecuteAsync(IActivityNodeContext context);
        Task AfterNodeExecuteAsync(IActivityNodeContext context);

        Task BeforeFlowActivateAsync(IActivityFlowContext context, bool activated);
        Task AfterFlowActivateAsync(IActivityFlowContext context);

        Task BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context);
        Task AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult);

        Task BeforeFlowTransformAsync<TToken>(ITransformationContext<TToken> context);
        Task AfterFlowTransformAsync<TToken>(ITransformationContext<TToken> context);
    }
}
