﻿using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class InitializedCompositeStateBuilderElsePayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IInitializedCompositeStateBuilder AddElseInternalTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
