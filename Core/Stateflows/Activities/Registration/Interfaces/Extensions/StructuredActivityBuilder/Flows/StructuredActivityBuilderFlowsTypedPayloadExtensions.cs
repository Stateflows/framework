﻿using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class StructuredActivityBuilderFlowsTypedPayloadExtensions
    {
        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload, TTargetNode>(this IStructuredActivityBuilder builder, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload, TObjectFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, TObjectFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow>(this IStructuredActivityBuilder builder, string targetNodeName)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            => (builder as IActionBuilder).AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(targetNodeName) as IStructuredActivityBuilder;

        public static IStructuredActivityBuilder AddDataFlow<TTokenPayload, TTransformedTokenPayload, TFlow, TTargetNode>(this IStructuredActivityBuilder builder)
            where TFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
