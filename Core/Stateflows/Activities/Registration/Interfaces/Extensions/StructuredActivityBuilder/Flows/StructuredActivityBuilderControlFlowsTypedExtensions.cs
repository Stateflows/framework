using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddControlFlow<TTargetNode>(this IStructuredActivityBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddControlFlow<TControlFlow>(this IStructuredActivityBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddControlFlow<TControlFlow, TTargetNode>(this IStructuredActivityBuilder builder, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
