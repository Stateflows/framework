using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedFinalizedCompositeStateBuilderElsePayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseDataTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedFinalizedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
