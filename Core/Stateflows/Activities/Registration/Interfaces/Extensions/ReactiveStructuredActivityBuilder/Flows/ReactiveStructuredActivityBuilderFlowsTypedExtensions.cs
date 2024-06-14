using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderFlowsTypedExtensions
    {
        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTargetNode>(this IReactiveStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
