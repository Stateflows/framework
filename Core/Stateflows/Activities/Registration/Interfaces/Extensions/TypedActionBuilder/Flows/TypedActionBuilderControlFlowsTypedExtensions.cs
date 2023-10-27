using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class TypedActionBuilderControlFlowsTypedExtensions
    {
        public static ITypedActionBuilder AddControlFlow<TTargetNode>(this ITypedActionBuilder builder, ControlFlowBuilderAction buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddControlFlow(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static ITypedActionBuilder AddControlFlow<TControlFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as ITypedActionBuilder;

        public static ITypedActionBuilder AddControlFlow<TControlFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TControlFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
