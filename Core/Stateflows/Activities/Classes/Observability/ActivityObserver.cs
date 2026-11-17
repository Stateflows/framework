using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Base observer that forwards activity operations to their next callbacks by default.</summary>
    public abstract class ActivityObserver : IActivityObserver
    {
        /// <inheritdoc/>
        public virtual Task OnActionAsync(IActivityActionObservationContext actionContext, Func<Task> next) => next();
        /// <inheritdoc/>
        public virtual Task OnPredicateAsync(IActivityPredicateObservationContext predicateContext, Func<Task<bool>> next) => next();

        /// <inheritdoc/>
        public virtual Task OnActivityInitializeAsync(IActivityInitializationContext initializationContext, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnActivityFinalizeAsync(IActivityFinalizationContext finalizationContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeInitializeAsync(IActivityNodeContext nodeContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeFinalizeAsync(IActivityNodeContext nodeContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeActivateAsync(IActivityNodeContext nodeContext, bool activated, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnNodeExecuteAsync(IActivityNodeContext nodeContext, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowActivateAsync(IActivityBeforeFlowContext flowContext, Func<Task<IActivityAfterFlowContext>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> guardContext, Func<Task<bool>> next)
            => next();

        /// <inheritdoc/>
        public virtual Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> transformationContext, Func<Task<TTransformedToken>> next)
            => next();
    }
}
