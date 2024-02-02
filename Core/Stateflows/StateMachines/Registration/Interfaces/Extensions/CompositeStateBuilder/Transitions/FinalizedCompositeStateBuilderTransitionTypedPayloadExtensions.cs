using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class FinalizedCompositeStateBuilderTransitionTypedPayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddTransition<TEventPayload, TTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddTransition<TEventPayload, TTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static IFinalizedCompositeStateBuilder AddTransition<TEventPayload, TTargetState>(this IFinalizedCompositeStateBuilder builder, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
