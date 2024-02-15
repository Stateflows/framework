using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class InitializedCompositeStateBuilderTransitionTypedPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddDataTransition<TEventPayload, TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddDataTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddDataTransition<TEventPayload, TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static IInitializedCompositeStateBuilder AddDataTransition<TEventPayload, TTargetState>(this IInitializedCompositeStateBuilder builder, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
