using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class CompositeStateBuilderTransitionTypedPayloadExtensions
    {
        public static ICompositeStateBuilder AddTransition<TEventPayload, TTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : State
            => AddTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddTransition<TEventPayload, TTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static ICompositeStateBuilder AddTransition<TEventPayload, TTargetState>(this ICompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
