﻿using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderDefaultTransitionTypedExtensions
    {
        public static IStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IStateBuilder AddDefaultTransition<TTransition>(this IStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IStateBuilder AddDefaultTransition<TTargetState>(this IStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
