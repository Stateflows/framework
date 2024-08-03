using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class TypedActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TTargetNode>(this ITypedActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TControlFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TControlFlow : class, IBaseControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as ITypedActionBuilder;

        [DebuggerHidden]
        public static ITypedActionBuilder AddControlFlow<TControlFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TControlFlow : class, IBaseControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNode<TTargetNode>.Name);
    }
}
