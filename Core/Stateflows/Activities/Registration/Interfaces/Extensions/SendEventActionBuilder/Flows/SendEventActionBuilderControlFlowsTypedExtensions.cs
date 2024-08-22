using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class SendEventActionBuilderControlFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static ISendEventActionBuilder AddControlFlow<TTargetNode>(this ISendEventActionBuilder builder, ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static ISendEventActionBuilder AddControlFlow<TControlFlow>(this ISendEventActionBuilder builder, string targetNodeName)
            where TControlFlow : class, IControlFlow
        {
            (builder as IInternal).Services.AddServiceType<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        [DebuggerHidden]
        public static ISendEventActionBuilder AddControlFlow<TFlow, TTargetNode>(this ISendEventActionBuilder builder)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
