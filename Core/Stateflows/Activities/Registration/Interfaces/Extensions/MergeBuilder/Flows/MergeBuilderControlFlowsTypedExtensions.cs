using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class MergeBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static void AddControlFlow<TTargetNode>(this IMergeBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static void AddControlFlow<TControlFlow>(this IMergeBuilder builder, string targetNodeName)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static void AddControlFlow<TFlow, TTargetNode>(this IMergeBuilder builder)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
