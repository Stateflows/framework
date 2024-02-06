using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class DecisionBuilderElseControlFlowsTypedExtensions
    {
        public static IDecisionBuilder AddElseFlow<TTargetNode>(this IDecisionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddElseFlow(ActivityNodeInfo<TTargetNode>.Name, b => buildAction?.Invoke(b as IControlFlowBuilder));

        public static IDecisionBuilder AddElseFlow<TControlFlow>(this IDecisionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            return builder.AddElseFlow(
            targetNodeName,
                b => (b as IControlFlowBuilder).AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IDecisionBuilder AddElseFlow<TFlow, TTargetNode>(this IDecisionBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddElseFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
