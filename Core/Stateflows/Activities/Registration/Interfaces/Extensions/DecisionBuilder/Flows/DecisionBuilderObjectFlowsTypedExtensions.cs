using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderObjectFlowsTypedExtensions
    {
        //public static IDecisionBuilder<TToken> AddFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
        //    where TTargetNode : ActivityNode
        //    => builder.AddFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        //public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
        //    where TFlow : Flow<TToken>
        //{
        //    (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

        //    return builder.AddFlow(
        //        targetNodeName,
        //        b => b.AddObjectFlowEvents<TFlow, TToken>()
        //    );
        //}

        //public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
        //    where TFlow : Flow<TToken>
        //    where TTargetNode : ActivityNode
        //    => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        //public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
        //    where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        //{
        //    (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

        //    return builder.AddFlow(
        //        targetNodeName,
        //        b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
        //    );
        //}

        //public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
        //    where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        //    where TTargetNode : ActivityNode
        //    => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
