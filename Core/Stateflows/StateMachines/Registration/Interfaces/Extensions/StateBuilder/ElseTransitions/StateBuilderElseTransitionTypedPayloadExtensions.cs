using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class StateBuilderElseTransitionTypedPayloadExtensions
    {
        public static IStateBuilder AddElseTransition<TEventPayload, TElseTransition, TTargetState>(this IStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddElseTransition<TEventPayload, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        public static IStateBuilder AddElseTransition<TEventPayload, TElseTransition>(this IStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, Event<TEventPayload>>();

            return builder.AddElseTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, Event<TEventPayload>>()
            );
        }

        public static IStateBuilder AddElseTransition<TEventPayload, TTargetState>(this IStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
