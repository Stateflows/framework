using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class JoinBuilderControlFlowsTypedExtensions
    {
        public static void AddControlFlow<TTargetNode>(this IJoinBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddControlFlow<TControlFlow>(this IJoinBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
        {
            (builder as IInternal).Services.RegisterControlFlow<TControlFlow>();

            builder.AddControlFlow(
                targetNodeName,
                b => b.AddControlFlowEvents<TControlFlow>()
            );
        }

        public static void AddControlFlow<TFlow, TTargetNode>(this IJoinBuilder builder)
            where TFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
