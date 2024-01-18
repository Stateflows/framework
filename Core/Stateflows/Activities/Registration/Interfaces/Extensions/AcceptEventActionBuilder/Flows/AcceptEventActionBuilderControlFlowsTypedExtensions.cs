using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class AcceptEventActionBuilderControlFlowsTypedExtensions
    {
        public static IAcceptEventActionBuilder AddControlFlow<TTargetNode>(this IAcceptEventActionBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IAcceptEventActionBuilder AddControlFlow<TControlFlow>(this IAcceptEventActionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IAcceptEventActionBuilder AddControlFlow<TFlow, TTargetNode>(this IAcceptEventActionBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
