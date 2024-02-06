using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedFinalizedCompositeStateBuilderElseTransitionTypedPayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddElseTransition<TEventPayload, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, Event<TEventPayload>>();

            return builder.AddElseTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, Event<TEventPayload>>()
            );
        }

        public static ITypedFinalizedCompositeStateBuilder AddElseTransition<TEventPayload, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
