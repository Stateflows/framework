using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderFlowsTypedExtensions
    {
        public static IStructuredActivityBuilder AddObjectFlow<TToken, TTargetNode>(this IStructuredActivityBuilder builder, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IStructuredActivityBuilder AddObjectFlow<TToken, TObjectFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TObjectFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddObjectFlow<TToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IStructuredActivityBuilder AddObjectFlow<TToken, TTransformedToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddObjectFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
