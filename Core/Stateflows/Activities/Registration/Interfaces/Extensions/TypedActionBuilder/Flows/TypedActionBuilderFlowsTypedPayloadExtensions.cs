using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class TypedActionBuilderFlowsTypedPayloadExtensions
    {
        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTargetNode>(this ITypedActionBuilder builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TObjectFlow : Flow<Token<TTokenPayload>>
            => (builder as IActionBuilder).AddFlow<Token<TTokenPayload>, TObjectFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : Flow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            => (builder as IActionBuilder).AddFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
