using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Base observer that forwards activity operations to their next callbacks by default.</summary>
    public abstract class ActivityObserver : IActivityObserver
    {
        /// <inheritdoc/>
        public virtual Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeInitializeAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeFinalizeAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowActivateAsync(IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
            => next();
    }
}
