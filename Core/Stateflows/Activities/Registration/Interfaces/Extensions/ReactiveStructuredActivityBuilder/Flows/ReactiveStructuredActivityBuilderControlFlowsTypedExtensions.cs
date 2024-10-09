using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class ReactiveStructuredActivityBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TTargetNode>(this IReactiveStructuredActivityBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName, buildAction) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
