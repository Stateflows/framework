using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderObjectFlowsTypedExtensions
    {
        public static IActionBuilder AddFlow<TToken, TTargetNode>(this IActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddFlow<TToken, TFlow>(this IActionBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static IActionBuilder AddFlow<TToken, TFlow, TTargetNode>(this IActionBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IActionBuilder builder, string targetNodeName)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IActionBuilder builder)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
