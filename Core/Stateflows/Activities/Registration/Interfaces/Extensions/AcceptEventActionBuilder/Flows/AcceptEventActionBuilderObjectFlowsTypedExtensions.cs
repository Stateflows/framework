using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderObjectFlowsTypedExtensions
    {
        public static IAcceptEventActionBuilder AddFlow<TToken, TTargetNode>(this IAcceptEventActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            // where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddFlow<TToken, TFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
