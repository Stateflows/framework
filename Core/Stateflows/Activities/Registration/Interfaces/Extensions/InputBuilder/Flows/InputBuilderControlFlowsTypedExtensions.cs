using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class InputBuilderControlFlowsTypedExtensions
    {
        public static IInputBuilder AddControlFlow<TControlFlow>(this IInputBuilder builder, string targetNodeName)
            where TControlFlow : ControlFlow
            => (builder as IActionBuilder).AddControlFlow<TControlFlow>(targetNodeName) as IInputBuilder;

        public static IInputBuilder AddControlFlow<TControlFlow, TTargetNode>(this IInputBuilder builder)
            where TControlFlow : ControlFlow
            where TTargetNode : ActivityNode
            => builder.AddControlFlow<TControlFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
