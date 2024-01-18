using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class DecisionBuilderElseObjectFlowsTypedPayloadExtensions
    {
        public static IDecisionBuilder<Token<TTokenPayload>> AddElseFlow<TTokenPayload, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddElseFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<Token<TTokenPayload>>));

        public static IDecisionBuilder<Token<TTokenPayload>> AddElseFlow<TTokenPayload, TObjectFlow>(this IDecisionBuilder<Token<TTokenPayload>> builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, Token<TTokenPayload>>();

            return builder.AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<Token<TTokenPayload>>).AddObjectFlowEvents<TObjectFlow, Token<TTokenPayload>>()
            );
        }

        public static IDecisionBuilder<Token<TTokenPayload>> AddElseFlow<TTokenPayload, TObjectFlow, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddElseFlow<Token<TTokenPayload>, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IDecisionBuilder<Token<TTokenPayload>> AddElseFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow>(this IDecisionBuilder<Token<TTokenPayload>> builder, string targetNodeName)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>();

            return builder.AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<Token<TTokenPayload>>).AddObjectTransformationFlowEvents<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>()
            );
        }

        public static IDecisionBuilder<Token<TTokenPayload>> AddElseFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddElseFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
