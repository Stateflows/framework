using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    internal abstract class ActivityPlugin : IActivityPlugin
    {
        public virtual Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => next();

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

        public virtual Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
            => next();

        public virtual Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
            => next();

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

        public virtual Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.FromResult(false);
    }
}
