using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TTargetNode>(this IDecisionBuilder builder, ControlFlowBuildAction buildAction)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TControlFlow>(this IDecisionBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TFlow, TTargetNode>(this IDecisionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
