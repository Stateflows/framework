using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class InputBuilderFlowsTypedExtensions
    {
        public static IInputBuilder AddTokenFlow<TToken, TTargetNode>(this IInputBuilder builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IInputBuilder AddTokenFlow<TToken, TObjectFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TObjectFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddTokenFlow<TToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IInputBuilder AddTokenFlow<TToken, TTransformedToken, TFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddTokenFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
