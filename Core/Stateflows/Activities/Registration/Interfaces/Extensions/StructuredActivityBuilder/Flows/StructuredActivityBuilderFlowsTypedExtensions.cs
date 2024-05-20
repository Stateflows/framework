using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderFlowsTypedExtensions
    {
        public static IStructuredActivityBuilder AddFlow<TToken, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IStructuredActivityBuilder AddFlow<TToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddFlow<TToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
