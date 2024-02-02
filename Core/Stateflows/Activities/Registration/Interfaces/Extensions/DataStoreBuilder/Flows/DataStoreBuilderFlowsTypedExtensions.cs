using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DataStoreBuilderFlowsTypedExtensions
    {
        public static void AddTokenFlow<TToken, TTargetNode>(this IDataStoreBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TToken : Token, new()
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddTokenFlow<TToken, TObjectFlow>(this IDataStoreBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, TToken>();
            builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, TToken>()
            );
        }

        public static void AddTokenFlow<TToken, TObjectFlow, TTargetNode>(this IDataStoreBuilder builder)
            where TToken : Token, new()
            where TObjectFlow : TokenFlow<TToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(this IDataStoreBuilder builder, string targetNodeName)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, TToken, TTransformedToken>();
            builder.AddTokenFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, TToken, TTransformedToken>()
            );
        }

        public static void AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow, TTargetNode>(this IDataStoreBuilder builder)
            where TToken : Token, new()
            where TTransformedToken : Token, new()
            where TObjectTransformationFlow : TokenTransformationFlow<TToken, TTransformedToken>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<TToken, TTransformedToken, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
