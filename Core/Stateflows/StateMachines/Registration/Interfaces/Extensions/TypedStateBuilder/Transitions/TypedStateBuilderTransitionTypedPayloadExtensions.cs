using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedStateBuilderTransitionTypedPayloadExtensions
    {
        public static ITypedStateBuilder AddTransition<TEventPayload, TTransition, TTargetState>(this ITypedStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        public static ITypedStateBuilder AddTransition<TEventPayload, TTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static ITypedStateBuilder AddTransition<TEventPayload, TTargetState>(this ITypedStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
