using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderObjectFlowsTypedExtensions
    {
        public static IActionBuilder AddFlow<TToken, TTargetNode>(this IActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddFlow<TToken, TObjectFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IActionBuilder AddFlow<TToken, TObjectFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
