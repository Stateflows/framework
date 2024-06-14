using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class JoinBuilderFlowsTypedExtensions
    {
        public static void AddFlow<TToken, TTargetNode>(this IJoinBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddFlow<TToken, TFlow>(this IJoinBuilder builder, string targetNodeName)
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static void AddFlow<TToken, TFlow, TTargetNode>(this IJoinBuilder builder)
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IJoinBuilder builder, string targetNodeName)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IJoinBuilder builder)
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
