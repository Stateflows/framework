using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderObjectFlowsTypedExtensions
    {
        public static IAcceptEventActionBuilder AddFlow<TToken, TTargetNode>(this IAcceptEventActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddFlow<TToken, TFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TTransformationFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterTransformationFlow<TTransformationFlow, TToken, TTransformedToken>();

            return builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static IAcceptEventActionBuilder AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
