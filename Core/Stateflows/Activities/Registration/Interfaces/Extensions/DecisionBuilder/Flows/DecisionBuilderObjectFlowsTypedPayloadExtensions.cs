using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class DecisionBuilderObjectFlowsTypedPayloadExtensions
    {
        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload, TObjectFlow>(this IDecisionBuilder<Token<TTokenPayload>> builder, string targetNodeName)
            where TObjectFlow : Flow<Token<TTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, Token<TTokenPayload>>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, Token<TTokenPayload>>()
            );
        }

        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload, TObjectFlow, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder)
            where TObjectFlow : Flow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow>(this IDecisionBuilder<Token<TTokenPayload>> builder, string targetNodeName)
            where TObjectTransformationFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>()
            );
        }

        public static IDecisionBuilder<Token<TTokenPayload>> AddFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<Token<TTokenPayload>> builder)
            where TObjectTransformationFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
