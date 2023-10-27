using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class MergeBuilderFlowsTypedExtensions
    {
        public static void AddObjectFlow<TToken, TTargetNode>(this IMergeBuilder builder, FlowBuilderAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddObjectFlow<TToken, TObjectFlow>(this IMergeBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();
            builder.AddObjectFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static void AddObjectFlow<TToken, TObjectFlow, TTargetNode>(this IMergeBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : ObjectFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IMergeBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();
            builder.AddObjectFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static void AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IMergeBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : ObjectTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddObjectFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
