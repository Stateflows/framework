using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Base activity exception handler that acknowledges no failures by default.</summary>
    /// <remarks>Override only the callbacks for failure types the application explicitly acknowledges; returning true does not restore failed execution.</remarks>
    public abstract class ActivityExceptionHandler : IActivityExceptionHandler
    {
        /// <inheritdoc/>
        public virtual Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => Task.FromResult(false);

        /// <inheritdoc/>
        public virtual Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.FromResult(false);

    }
}
