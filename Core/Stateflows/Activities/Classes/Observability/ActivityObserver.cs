using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityObserver : IActivityObserver
    {
        public virtual Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
            => next();

        public virtual Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
            => next();

        public virtual Task OnNodeInitializeAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        public virtual Task OnNodeFinalizeAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        public virtual Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next)
            => next();

        public virtual Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next)
            => next();

        public virtual Task OnFlowActivateAsync(IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next)
            => next();

        public virtual Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
            => next();

        public virtual Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
            => next();
    }
}
