using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderObjectFlowsTypedExtensions
    {
        public static IAcceptEventActionBuilder AddFlow<TToken, TTargetNode>(this IAcceptEventActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddFlow<TToken, TObjectFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TObjectFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
