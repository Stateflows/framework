﻿using System.Diagnostics;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public static class StructuredActivityBuilderFlowsTypedExtensions
    {
        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFlow<TToken, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFlow<TToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            => (builder as IActionBuilder).AddFlow<TToken, TFlow>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFlow<TToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuildAction<TToken> buildAction = null)
            where TFlow : class, IFlow<TToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            => (builder as IActionBuilder).AddFlow<TToken, TTransformedToken, TFlow>(targetNodeName, buildAction) as IStructuredActivityBuilder;

        [DebuggerHidden]
        public static IStructuredActivityBuilder AddFlow<TToken, TTransformedToken, TFlow, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuildAction<TTransformedToken> buildAction = null)
            where TFlow : class, IFlowTransformation<TToken, TTransformedToken>
            where TTargetNode : class, IActivityNode
            => builder.AddFlow<TToken, TTransformedToken, TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
