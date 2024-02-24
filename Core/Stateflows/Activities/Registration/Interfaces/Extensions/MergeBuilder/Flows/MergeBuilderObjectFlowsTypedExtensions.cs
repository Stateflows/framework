using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class MergeBuilderObjectFlowsTypedExtensions
    {
        public static void AddFlow<TToken, TTargetNode>(this IMergeBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddFlow<TToken, TObjectFlow>(this IMergeBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static void AddFlow<TToken, TObjectFlow, TTargetNode>(this IMergeBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : Flow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IMergeBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static void AddFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IMergeBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
