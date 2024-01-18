﻿using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddTransition<TEvent, TTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            where TTargetState : State
            => AddTransition<TEvent, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddTransition<TEvent, TTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, TEvent>();

            return builder.AddTransition<TEvent>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, TEvent>()
            );
        }

        public static IFinalizedCompositeStateBuilder AddTransition<TEvent, TTargetState>(this IFinalizedCompositeStateBuilder builder, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
