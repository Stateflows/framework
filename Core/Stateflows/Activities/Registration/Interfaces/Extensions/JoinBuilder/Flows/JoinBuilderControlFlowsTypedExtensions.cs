using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class JoinBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static void AddControlFlow<TTargetNode>(this IJoinBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static void AddControlFlow<TControlFlow>(this IJoinBuilder builder, string targetNodeName)
            where TControlFlow : class, IBaseControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static void AddControlFlow<TFlow, TTargetNode>(this IJoinBuilder builder)
            where TFlow : class, IBaseControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
