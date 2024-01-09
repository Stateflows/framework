using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class DecisionBuilderElseControlFlowsTypedExtensions
    {
        public static IDecisionBuilder AddElseControlFlow<TTargetNode>(this IDecisionBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddElseControlFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IControlFlowBuilder));

        public static IDecisionBuilder AddElseControlFlow<TControlFlow>(this IDecisionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            return builder.AddElseControlFlow(
            targetNodeName,
                b => (b as IControlFlowBuilder).AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IDecisionBuilder AddElseControlFlow<TFlow, TTargetNode>(this IDecisionBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddElseControlFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
