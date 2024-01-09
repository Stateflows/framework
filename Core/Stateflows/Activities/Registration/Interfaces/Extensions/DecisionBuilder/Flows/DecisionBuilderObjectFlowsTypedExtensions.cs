using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderObjectFlowsTypedExtensions
    {
        public static IDecisionBuilder<TToken> AddObjectFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IDecisionBuilder<TToken> AddObjectFlow<TToken, TObjectFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddObjectFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddObjectFlow<TToken, TObjectFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IDecisionBuilder<TToken> AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddObjectFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
