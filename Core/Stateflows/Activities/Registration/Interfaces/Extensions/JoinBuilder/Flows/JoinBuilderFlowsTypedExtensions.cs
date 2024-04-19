using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class JoinBuilderFlowsTypedExtensions
    {
        public static void AddFlow<TToken, TTargetNode>(this IJoinBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            // where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddFlow<TToken, TFlow>(this IJoinBuilder builder, string targetNodeName)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TFlow, TToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        public static void AddFlow<TToken, TFlow, TTargetNode>(this IJoinBuilder builder)
            // where TToken : Token, new()
            where TFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IJoinBuilder builder, string targetNodeName)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IJoinBuilder builder)
            // where TToken : Token, new()
            ////where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
