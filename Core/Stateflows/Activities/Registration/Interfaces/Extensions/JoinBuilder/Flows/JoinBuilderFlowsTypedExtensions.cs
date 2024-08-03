using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class JoinBuilderFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static void AddFlow<TToken, TTargetNode>(this IJoinBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static void AddFlow<TToken, TFlow>(this IJoinBuilder builder, string targetNodeName)
            where TFlow : class, IBaseFlow<TToken>
        {
            (builder as IInternal).Services.AddServiceType<TFlow>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        [DebuggerHidden]
        public static void AddFlow<TToken, TFlow, TTargetNode>(this IJoinBuilder builder)
            where TFlow : class, IBaseFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name);

        [DebuggerHidden]
        public static void AddFlow<TToken, TTransformedToken, TTransformationFlow>(this IJoinBuilder builder, string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.AddServiceType<TTransformationFlow>();
            builder.AddFlow<TToken>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

        [DebuggerHidden]
        public static void AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IJoinBuilder builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }
}
