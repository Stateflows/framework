using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActionBuilderFlowsTypedExtensions
    {
        public static ITypedActionBuilder AddObjectFlow<TToken, TTargetNode>(this ITypedActionBuilder builder, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddObjectFlow<TToken, TObjectFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TObjectFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddObjectFlow<TToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TToken : Token, new()
            where TFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static ITypedActionBuilder AddObjectFlow<TToken, TTransformedToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddObjectFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
