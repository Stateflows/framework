﻿using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Typed.Data
{
    public static class JoinBuilderFlowsTypedPayloadExtensions
    {
        public static void AddDataFlow<TTokenPayload, TTargetNode>(this IJoinBuilder builder, ObjectFlowBuilderAction<Token<TTokenPayload>> buildAction = null)
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>>(ActivityNodeInfo<TTargetNode>.Name, buildAction);

        public static void AddDataFlow<TTokenPayload, TObjectFlow>(this IJoinBuilder builder, string targetNodeName)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectFlow<TObjectFlow, Token<TTokenPayload>>();
            builder.AddTokenFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectFlowEvents<TObjectFlow, Token<TTokenPayload>>()
            );
        }

        public static void AddDataFlow<TTokenPayload, TObjectFlow, TTargetNode>(this IJoinBuilder builder)
            where TObjectFlow : TokenFlow<Token<TTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, TObjectFlow>(ActivityNodeInfo<TTargetNode>.Name);

        public static void AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow>(this IJoinBuilder builder, string targetNodeName)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
        {
            (builder as IInternal).Services.RegisterObjectTransformationFlow<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>();
            builder.AddTokenFlow<Token<TTokenPayload>>(
                targetNodeName,
                b => b.AddObjectTransformationFlowEvents<TObjectTransformationFlow, Token<TTokenPayload>, Token<TTransformedTokenPayload>>()
            );
        }

        public static void AddDataFlow<TTokenPayload, TTransformedTokenPayload, TObjectTransformationFlow, TTargetNode>(this IJoinBuilder builder)
            where TObjectTransformationFlow : TokenTransformationFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>>
            where TTargetNode : ActivityNode
            => builder.AddTokenFlow<Token<TTokenPayload>, Token<TTransformedTokenPayload>, TObjectTransformationFlow>(ActivityNodeInfo<TTargetNode>.Name);
    }
}
