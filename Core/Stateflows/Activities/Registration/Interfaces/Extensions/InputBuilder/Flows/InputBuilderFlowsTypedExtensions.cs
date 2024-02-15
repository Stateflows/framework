using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class InputBuilderFlowsTypedExtensions
    {
        public static IInputBuilder AddFlow<TToken, TTargetNode>(this IInputBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IInputBuilder AddFlow<TToken, TObjectFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TObjectFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddFlow<TToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IInputBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
