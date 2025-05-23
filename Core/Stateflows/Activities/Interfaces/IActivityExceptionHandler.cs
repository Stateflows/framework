using System;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityExceptionHandler
    {
        bool OnActivityInitializationException(IActivityInitializationContext context, Exception exception);

        bool OnActivityFinalizationException(IActivityFinalizationContext context, Exception exception);

        bool OnNodeInitializationException(IActivityNodeContext context, Exception exception);

        bool OnNodeFinalizationException(IActivityNodeContext context, Exception exception);

        bool OnNodeExecutionException(IActivityNodeContext context, Exception exception);

        bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception);

        bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception);

    }
}
