using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ReactiveStructuredActivityBuilderFlowsTypedPayloadExtensions
    {
        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload, TTargetNode>(this IReactiveStructuredActivityBuilder builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TObjectFlow : Flow<Token<TTokenPayload>>
            => (builder as IActionBuilder).AddFlow<Token<TTokenPayload>, TObjectFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TFlow : Flow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            => (builder as IActionBuilder).AddFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TFlow : TransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
