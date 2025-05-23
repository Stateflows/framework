using System;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public abstract class ActivityExceptionHandler : IActivityExceptionHandler
    {
        public virtual bool OnActivityInitializationException(IActivityInitializationContext context, Exception exception)
            => false;

        public virtual bool OnActivityFinalizationException(IActivityFinalizationContext context, Exception exception)
            => false;

        public virtual bool OnNodeInitializationException(IActivityNodeContext context, Exception exception)
            => false;

        public virtual bool OnNodeFinalizationException(IActivityNodeContext context, Exception exception)
            => false;

        public virtual bool OnNodeExecutionException(IActivityNodeContext context, Exception exception)
            => false;

        public virtual bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception)
            => false;

        public virtual bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => false;

    }
}
