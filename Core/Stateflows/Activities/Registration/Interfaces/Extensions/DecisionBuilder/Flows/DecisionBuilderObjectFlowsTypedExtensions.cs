using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderObjectFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
        {
            (builder as IInternal).Services.AddServiceType<TFlow>();

            return builder.AddFlow(
                targetNodeName,
                b =>
                {
                    b.AddObjectFlowEvents<TFlow, TToken>();
                    buildAction?.Invoke(b);
                }
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TFlow, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TTransformationFlow>(this IDecisionBuilder<TToken> builder, string targetNodeName, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
        {
            (builder as IInternal).Services.AddServiceType<TTransformationFlow>();

            return builder.AddFlow(
                targetNodeName,
                b =>
                {
                    b.AddObjectTransformationFlowEvents<TTransformationFlow, TToken, TTransformedToken>();
                    buildAction?.Invoke(b as IObjectFlowBuilder<TTransformedToken>);
                }
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder<TToken> AddFlow<TToken, TTransformedToken, TTransformationFlow, TTargetNode>(this IDecisionBuilder<TToken> builder, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TTransformationFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TTransformationFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
