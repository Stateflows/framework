using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedInitializedCompositeStateBuilderElseTransitionTypedPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            where TTargetState : State
            => AddElseTransition<TEventPayload, TElseTransition>(builder, StateInfo<TTargetState>.Name);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
        {
            (builder as IInternal).Services.RegisterElseTransition<TElseTransition, Event<TEventPayload>>();

            return builder.AddElseTransition<Event<TEventPayload>>(
                targetVertexName,
                t => t.AddElseTransitionEvents<TElseTransition, Event<TEventPayload>>()
            );
        }

        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEventPayload, TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddElseTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
