using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class FinalizedCompositeStateBuilderElseTransitionTypedPayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            where TTargetState : State
            => AddElseTransition<TEventPayload, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, Event<TEventPayload>>();

            return builder.AddElseTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, Event<TEventPayload>>()
            );
        }

        public static IFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
