using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InputBuilderFlowsTypedExtensions
    {
        public static IInputBuilder AddObjectFlow<TToken, TTargetNode>(this IInputBuilder builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IInputBuilder AddObjectFlow<TToken, TObjectFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TObjectFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddObjectFlow<TToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IInputBuilder AddObjectFlow<TToken, TTransformedToken, TFlow>(this IInputBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddObjectFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddObjectFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IInputBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
