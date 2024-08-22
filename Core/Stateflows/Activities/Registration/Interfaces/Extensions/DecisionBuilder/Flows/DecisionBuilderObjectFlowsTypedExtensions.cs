using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderObjectFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TFlow : class, IFlow<TToken>
        {
            (builder as IInternal).Services.AddServiceType<TFlow>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddObjectFlowEvents<TFlow, TToken>()
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name);

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.AddServiceType<TTransformationFlow>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>()
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name);
    }
}
