using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderFlowsTypedExtensions
    {
        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTargetNode>(this IReactiveStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TObjectFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TObjectFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
