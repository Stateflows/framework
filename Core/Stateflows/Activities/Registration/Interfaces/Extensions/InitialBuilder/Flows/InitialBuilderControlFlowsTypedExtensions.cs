using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InitialBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IInitialBuilder AddControlFlow<TTargetNode>(this IInitialBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IInitialBuilder AddControlFlow<TControlFlow>(this IInitialBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
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
        public static IInitialBuilder AddControlFlow<TFlow, TTargetNode>(this IInitialBuilder builder, ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(builder, ActivityNode<TTargetNode>.Name, buildAction);
    }
}
