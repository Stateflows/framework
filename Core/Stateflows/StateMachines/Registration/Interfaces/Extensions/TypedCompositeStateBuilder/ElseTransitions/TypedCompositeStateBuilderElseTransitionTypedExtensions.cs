﻿using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateBuilderElseTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            where TTargetState : BaseState
            => AddElseTransition<TEvent, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, TEvent>();

            return builder.AddElseTransition<TEvent>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, TEvent>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : BaseState
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
