﻿using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
