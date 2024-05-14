using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class TypedActionBuilderFlowsTypedExtensions
    {
        public static ITypedActionBuilder AddFlow<TToken, TTargetNode>(this ITypedActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddFlow<TToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddFlow<TToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static ITypedActionBuilder AddFlow<TToken, TTransformedToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
