using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class AcceptEventActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TTargetNode>(this IAcceptEventActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TControlFlow>(this IAcceptEventActionBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );
        }

        [DebuggerHidden]
        public static IAcceptEventActionBuilder AddControlFlow<TFlow, TTargetNode>(this IAcceptEventActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
