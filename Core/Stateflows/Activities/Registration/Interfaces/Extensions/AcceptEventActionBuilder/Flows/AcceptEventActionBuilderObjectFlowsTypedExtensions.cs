using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderObjectFlowsTypedExtensions
    {
        public static IAcceptEventActionBuilder AddTokenFlow<TToken, TTargetNode>(this IAcceptEventActionBuilder builder, ObjectFlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddTokenFlow<TToken, TObjectFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IAcceptEventActionBuilder AddTokenFlow<TToken, TObjectFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IAcceptEventActionBuilder AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IAcceptEventActionBuilder AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
