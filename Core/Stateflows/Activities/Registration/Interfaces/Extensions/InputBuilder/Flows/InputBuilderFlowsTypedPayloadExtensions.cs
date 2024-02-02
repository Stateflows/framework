using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class InputBuilderFlowsTypedPayloadExtensions
    {
        public static IInputBuilder AddDataFlow<TTokenPayload, TTargetNode>(this IInputBuilder builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IInputBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this IInputBuilder builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, TObjectFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddDataFlow<TTokenPayload, TFlow, TTargetNode>(this IInputBuilder builder)
            where TFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IInputBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow>(this IInputBuilder builder, string targetNodeName)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow, TTargetNode>(this IInputBuilder builder)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
