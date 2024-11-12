using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityExceptionHandler : IActivityExceptionHandler
    {
        public virtual Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => Task.FromResult(false);

        public virtual Task<bool> OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.FromResult(false);

    }
}
