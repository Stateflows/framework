using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ActionBuilderObjectFlowsTypedExtensions
    {
        public static IActionBuilder AddObjectFlow<TToken, TTargetNode>(this IActionBuilder builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddObjectFlow<TToken, TObjectFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddObjectFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IActionBuilder AddObjectFlow<TToken, TObjectFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IActionBuilder AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddObjectFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IActionBuilder AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
