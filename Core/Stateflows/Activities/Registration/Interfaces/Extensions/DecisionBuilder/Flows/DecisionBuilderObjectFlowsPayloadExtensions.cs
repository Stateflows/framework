using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class DecisionBuilderObjectFlowsPayloadExtensions
    {
        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload>(this IDecisionBuilder<Token<TTokenPayload>> builder, string targetNodeName, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddFlow(targetNodeName, buildAction);
    }
}
