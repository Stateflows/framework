using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderElseObjectFlowsTypedExtensions
    {
        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
            // where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddElseFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IObjectFlowBuilder<TToken>));

        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return builder.AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddElseFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddElseFlow(
                targetNodeName,
                b => (b as IObjectFlowBuilder<TToken>).AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IDecisionBuilder<TToken> AddElseFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddElseFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
