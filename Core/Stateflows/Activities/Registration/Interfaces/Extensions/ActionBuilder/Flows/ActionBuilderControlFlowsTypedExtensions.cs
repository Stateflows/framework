using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderControlFlowsTypedExtensions
    {
        public static IActionBuilder AddControlFlow<TTargetNode>(this IActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IActionBuilder AddControlFlow<TControlFlow>(this IActionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IActionBuilder AddControlFlow<TFlow, TTargetNode>(this IActionBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
