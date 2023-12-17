﻿using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderElseTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            where TTargetState : State
            => AddElseTransition<TEvent, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, TEvent>();

            return builder.AddElseTransition<TEvent>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, TEvent>()
            );
        }

        public static ICompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ICompositeStateBuilder builder, ElseTransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : State
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
