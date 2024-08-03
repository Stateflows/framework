using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ReactiveStructuredActivityBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TTargetNode>(this IReactiveStructuredActivityBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow>(this IReactiveStructuredActivityBuilder builder, string targetNodeName)
            where TControlFlow : class, IBaseControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as IReactiveStructuredActivityBuilder;

        [DebuggerHidden]
        public static IReactiveStructuredActivityBuilder AddControlFlow<TControlFlow, TTargetNode>(this IReactiveStructuredActivityBuilder builder)
            where TControlFlow : class, IBaseControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNode<TTargetNode>.Name);
    }
}
