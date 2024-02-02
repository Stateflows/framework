using Stateflows.Common;
using Stateflows.Activities.Data;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class AcceptEventActionBuilderObjectFlowsTypedPayloadExtensions
    {
        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload, TTargetNode>(this IAcceptEventActionBuilder builder, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddDataFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, Token<TTokenPayload>>();

            return builder.AddTokenFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, Token<TTokenPayload>>()
            );
        }

        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload, TObjectFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddDataFlow<TTokenPayload, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>();

            return builder.AddTokenFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>()
            );
        }

        public static IAcceptEventActionBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TObjectTransformationFlow : TokenTransformationFlow<Event<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Event<TTokenPayload>, Token<TTransformedTokenPayload>, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
