using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class InitializedCompositeStateBuilderElseTransitionTypedPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddElseTransition<TEventPayload, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, Event<TEventPayload>>();

            return builder.AddElseTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, Event<TEventPayload>>()
            );
        }

        public static IInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TTargetState>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
