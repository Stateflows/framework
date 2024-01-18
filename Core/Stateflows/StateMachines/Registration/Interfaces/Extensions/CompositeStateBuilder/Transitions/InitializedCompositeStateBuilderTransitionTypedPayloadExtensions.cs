﻿using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class InitializedCompositeStateBuilderTransitionTypedPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddTransition<TEventPayload, TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : State
            => AddTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddTransition<TEventPayload, TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static IInitializedCompositeStateBuilder AddTransition<TEventPayload, TTargetState>(this IInitializedCompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
