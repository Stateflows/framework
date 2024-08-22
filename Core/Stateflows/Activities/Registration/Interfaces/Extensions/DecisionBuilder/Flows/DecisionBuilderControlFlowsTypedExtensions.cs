using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TTargetNode>(this IDecisionBuilder builder, ControlFlowBuildAction buildAction)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TControlFlow>(this IDecisionBuilder builder, string targetNodeName)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static IDecisionBuilder AddFlow<TFlow, TTargetNode>(this IDecisionBuilder builder)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
