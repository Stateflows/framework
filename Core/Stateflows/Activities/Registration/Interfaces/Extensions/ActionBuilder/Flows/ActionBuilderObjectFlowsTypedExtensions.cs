using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderObjectFlowsTypedExtensions
    {
        public static IActionBuilder AddTokenFlow<TToken, TTargetNode>(this IActionBuilder builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddTokenFlow<TToken, TObjectFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IActionBuilder AddTokenFlow<TToken, TObjectFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IActionBuilder AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IActionBuilder AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
