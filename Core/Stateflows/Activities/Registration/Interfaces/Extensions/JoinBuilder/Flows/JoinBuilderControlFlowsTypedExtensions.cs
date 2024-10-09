using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class JoinBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static void AddControlFlow<TTargetNode>(this IJoinBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static void AddControlFlow<TControlFlow>(this IJoinBuilder builder, string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            builder.AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );
        }

        [DebuggerHidden]
        public static void AddControlFlow<TFlow, TTargetNode>(this IJoinBuilder builder, ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
