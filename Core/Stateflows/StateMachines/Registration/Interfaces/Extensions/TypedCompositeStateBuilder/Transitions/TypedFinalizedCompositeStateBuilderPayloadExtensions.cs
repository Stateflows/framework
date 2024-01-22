﻿using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedFinalizedCompositeStateBuilderPayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedFinalizedCompositeStateBuilder AddInternalTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}