using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderControlFlowsTypedExtensions
    {
        public static IReactiveStructuredActivityBuilder AddControlFlow<TTargetNode>(this IReactiveStructuredActivityBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TControlFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
