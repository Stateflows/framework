using Stateflows.Common;
using Stateflows.Activities.Data;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class ActionBuilderObjectFlowsTypedPayloadExtensions
    {
        public static IActionBuilder AddDataFlow<TTokenPayload, TTargetNode>(this IActionBuilder builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddDataFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this IActionBuilder builder, string targetNodeName)
            where TObjectFlow : Flow<Token<TTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, Token<TTokenPayload>>();

            return builder.AddFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, Token<TTokenPayload>>()
            );
        }

        public static IActionBuilder AddDataFlow<TTokenPayload, TObjectFlow, TTargetNode>(this IActionBuilder builder)
            where TObjectFlow : Flow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddDataFlow<TTokenPayload, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow>(this IActionBuilder builder, string targetNodeName)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>();

            return builder.AddFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>()
            );
        }

        public static IActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow, TTargetNode>(this IActionBuilder builder)
            where TObjectTransformationFlow : TokenTransformationFlow<Event<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddFlow<Event<TTokenPayload>, Token<TTransformedTokenPayload>, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
