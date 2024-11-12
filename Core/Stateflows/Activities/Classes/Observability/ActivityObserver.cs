using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityObserver : IActivityObserver
    {
        public virtual Task BeforeActivityInitializeAsync(IActivityInitializationContext context)
            => Task.CompletedTask;
        public virtual Task AfterActivityInitializeAsync(IActivityInitializationContext context, bool initialized)
            => Task.CompletedTask;

        public virtual Task BeforeActivityFinalizeAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;
        public virtual Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
            => Task.CompletedTask;

        public virtual Task BeforeNodeInitializeAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        public virtual Task AfterNodeInitializeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public virtual Task BeforeNodeFinalizeAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        public virtual Task AfterNodeFinalizeAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public virtual Task BeforeNodeActivateAsync(IActivityNodeContext context, bool activated)
            => Task.CompletedTask;
        public virtual Task AfterNodeActivateAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public virtual Task BeforeNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;
        public virtual Task AfterNodeExecuteAsync(IActivityNodeContext context)
            => Task.CompletedTask;

        public virtual Task BeforeFlowActivateAsync(IActivityFlowContext context, bool activated)
            => Task.CompletedTask;
        public virtual Task AfterFlowActivateAsync(IActivityFlowContext context)
            => Task.CompletedTask;

        public virtual Task BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context)
            => Task.CompletedTask;
        public virtual Task AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult)
            => Task.CompletedTask;

        public virtual Task BeforeFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;
        public virtual Task AfterFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
            => Task.CompletedTask;
    }
}
