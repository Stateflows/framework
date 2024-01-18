using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class TypedActionBuilderFlowsTypedPayloadExtensions
    {
        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTargetNode>(this ITypedActionBuilder builder, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, TObjectFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
