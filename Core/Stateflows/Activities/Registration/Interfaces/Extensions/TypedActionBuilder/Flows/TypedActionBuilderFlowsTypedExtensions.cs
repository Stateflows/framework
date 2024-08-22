using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed
{
    public static class TypedActionBuilderFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedActionBuilder AddFlow<TToken, TTargetNode>(this ITypedActionBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static ITypedActionBuilder AddFlow<TToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : class, IFlow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        [DebuggerHidden]
        public static ITypedActionBuilder AddFlow<TToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name);

        [DebuggerHidden]
        public static ITypedActionBuilder AddFlow<TToken, TTransformedToken, TFlow>(this ITypedActionBuilder builder, string targetNodeName)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName) as ITypedActionBuilder;

        [DebuggerHidden]
        public static ITypedActionBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this ITypedActionBuilder builder)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNode<TTargetNode>.Name);
    }
}
