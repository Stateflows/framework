﻿using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class FinalizedCompositeStateBuilderPayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, string targetVertexName, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IFinalizedCompositeStateBuilder AddInternalTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
