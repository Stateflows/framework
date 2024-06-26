﻿using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class FinalizedCompositeStateBuilderElsePayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseDataTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IFinalizedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, ElseInternalTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
