using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TTargetNode>(this ITypedActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TControlFlow>(this ITypedActionBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName, buildAction) as ITypedActionBuilder;

        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TControlFlow, TTargetNode>(this ITypedActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
