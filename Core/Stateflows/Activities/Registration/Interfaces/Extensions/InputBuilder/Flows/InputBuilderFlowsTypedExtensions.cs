using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class InputBuilderFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IInputBuilder AddFlow<TToken, TTargetNode>(this IInputBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IInputBuilder AddFlow<TToken, TFlow>(this IInputBuilder builder, string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName, buildAction) as IInputBuilder;

        [DebuggerHidden]
        public static IInputBuilder AddFlow<TToken, TFlow, TTargetNode>(this IInputBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IInputBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IInputBuilder builder, string targetNodeName, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName, buildAction) as IInputBuilder;

        [DebuggerHidden]
        public static IInputBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IInputBuilder builder, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
