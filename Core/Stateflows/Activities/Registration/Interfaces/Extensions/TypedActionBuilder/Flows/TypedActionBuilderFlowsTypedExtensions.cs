using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Typed
{
    public static class TypedActionBuilderFlowsTypedExtensions
    {
        public static ITypedActionBuilder AddTokenFlow<TToken, TTargetNode>(this ITypedActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddTokenFlow<TToken, TObjectFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TObjectFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddTokenFlow<TToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TToken : Token, new()
            where TFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static ITypedActionBuilder AddTokenFlow<TToken, TTransformedToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddTokenFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
