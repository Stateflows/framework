using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderFlowsTypedExtensions
    {
        public static IStructuredActivityBuilder AddTokenFlow<TToken, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IStructuredActivityBuilder AddTokenFlow<TToken, TObjectFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TObjectFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddTokenFlow<TToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IStructuredActivityBuilder AddTokenFlow<TToken, TTransformedToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddTokenFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
