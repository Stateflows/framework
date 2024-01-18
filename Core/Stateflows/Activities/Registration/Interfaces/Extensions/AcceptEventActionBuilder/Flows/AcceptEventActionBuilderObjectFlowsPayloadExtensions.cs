using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class AcceptEventActionBuilderObjectFlowsPayloadExtensions
    {
        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload>(this IAcceptEventActionBuilder builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddTokenFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
