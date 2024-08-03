using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class InitialBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IInitialBuilder AddControlFlow<TTargetNode>(this IInitialBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IInitialBuilder AddControlFlow<TControlFlow>(this IInitialBuilder builder, string targetNodeName)
            where TControlFlow : class, IBaseControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();
            
            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static IInitialBuilder AddControlFlow<TFlow, TTargetNode>(this IInitialBuilder builder)
            where TFlow : class, IBaseControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(builder, ActivityNode<TTargetNode>.Name);
    }
}
