using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderControlFlowsTypedExtensions
    {
        public static IDecisionBuilder AddFlow<TTargetNode>(this IDecisionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IDecisionBuilder AddFlow<TControlFlow>(this IDecisionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            return builder.AddFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IDecisionBuilder AddFlow<TFlow, TTargetNode>(this IDecisionBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
