﻿using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedStateBuilder builder)
            where TElseTransition : ElseTransition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ITypedStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
