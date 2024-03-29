﻿using Stateflows.Common;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities.Data
{
    public static class ReactiveStructuredActivityBuilderFlowsPayloadExtensions
    {
        public static IReactiveStructuredActivityBuilder AddDataFlow<TTokenPayload>(this IReactiveStructuredActivityBuilder builder, string targetNodeName, ObjectFlowBuildAction<Token<TTokenPayload>> buildAction = null)
            => builder.AddFlow<Token<TTokenPayload>>(targetNodeName, buildAction);
    }
}
