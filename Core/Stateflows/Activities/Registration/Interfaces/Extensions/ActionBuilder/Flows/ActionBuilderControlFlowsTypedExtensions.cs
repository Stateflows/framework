using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class ActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IActionBuilder AddControlFlow<TTargetNode>(this IActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IActionBuilder AddControlFlow<TControlFlow>(this IActionBuilder builder, string targetNodeName)
            where TControlFlow : class, IBaseControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static IActionBuilder AddControlFlow<TFlow, TTargetNode>(this IActionBuilder builder)
            where TFlow : class, IBaseControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
