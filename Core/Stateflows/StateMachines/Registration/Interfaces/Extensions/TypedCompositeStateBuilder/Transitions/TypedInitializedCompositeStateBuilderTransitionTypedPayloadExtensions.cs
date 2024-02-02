using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedInitializedCompositeStateBuilderTransitionTypedPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEventPayload, TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            where TTargetState : BaseState
            => AddTransition<TEventPayload, TTransition>(builder, StateInfo<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEventPayload, TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterTransition<TTransition, Event<TEventPayload>>();

            return builder.AddTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddTransitionEvents<TTransition, Event<TEventPayload>>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddTransition<TEventPayload, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
