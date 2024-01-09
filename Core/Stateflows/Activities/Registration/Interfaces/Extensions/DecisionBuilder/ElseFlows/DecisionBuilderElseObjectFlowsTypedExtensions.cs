using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderElseObjectFlowsTypedExtensions
    {
        public static IDecisionBuilder<TToken> AddElseObjectFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddElseObjectFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        public static IDecisionBuilder<TToken> AddElseObjectFlow<TToken, TObjectFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddElseObjectFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddElseObjectFlow<TToken, TObjectFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddElseObjectFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IDecisionBuilder<TToken> AddElseObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddElseObjectFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddElseObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddElseObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
