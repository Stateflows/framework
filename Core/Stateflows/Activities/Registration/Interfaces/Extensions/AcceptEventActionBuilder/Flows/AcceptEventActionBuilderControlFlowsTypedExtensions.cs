using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TTargetNode>(this IAcceptEventActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TControlFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
