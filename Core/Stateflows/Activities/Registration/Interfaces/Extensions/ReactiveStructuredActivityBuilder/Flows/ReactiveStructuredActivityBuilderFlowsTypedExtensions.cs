using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderFlowsTypedExtensions
    {
        public static IReactiveStructuredActivityBuilder AddTokenFlow<TToken, TTargetNode>(this IReactiveStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddTokenFlow<TToken, TObjectFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TObjectFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddTokenFlow<TToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IReactiveStructuredActivityBuilder AddTokenFlow<TToken, TTransformedToken, TFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddTokenFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddTokenFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
