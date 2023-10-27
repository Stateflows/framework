using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InitialBuilderControlFlowsTypedExtensions
    {
        public static IInitialBuilder AddControlFlow<TTargetNode>(this IInitialBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IInitialBuilder AddControlFlow<TControlFlow>(this IInitialBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();
            
            return builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        public static IInitialBuilder AddControlFlow<TFlow, TTargetNode>(this IInitialBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => AddControlFlow<TFlow>(builder, ActivityNodeInfo<TTargetNode>.Name);
    }
}
