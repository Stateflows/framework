using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class StructuredActivityBuilderControlFlowsTypedExtensions
    {
        public static IStructuredActivityBuilder AddControlFlow<TTargetNode>(this IStructuredActivityBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IStructuredActivityBuilder AddControlFlow<TControlFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddControlFlow<TControlFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TControlFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
