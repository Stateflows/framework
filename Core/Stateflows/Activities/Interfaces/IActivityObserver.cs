using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationContext context)
            => Task.CompletedTask;
        Task AfterActivityInitializeAsync(IActivityInitializationContext context, bool initialized)
            => Task.CompletedTask;

        Task BeforeActivityFinalizeAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;
        Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;

        Task BeforeNodeInitializeAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        Task AfterNodeInitializeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task BeforeNodeFinalizeAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        Task AfterNodeFinalizeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task BeforeNodeActivateAsync(IActivityNodeContext context, bool activated)
            => Task.CompletedTask;
        Task AfterNodeActivateAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task BeforeNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        Task AfterNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        Task BeforeFlowActivateAsync(IActivityFlowContext context, bool activated)
            => Task.CompletedTask;
        Task AfterFlowActivateAsync(IActivityFlowContext context)
            => Task.CompletedTask;

        Task BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context)
            => Task.CompletedTask;
        Task AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult)
            => Task.CompletedTask;

        Task BeforeFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;
        Task AfterFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;
    }
}
