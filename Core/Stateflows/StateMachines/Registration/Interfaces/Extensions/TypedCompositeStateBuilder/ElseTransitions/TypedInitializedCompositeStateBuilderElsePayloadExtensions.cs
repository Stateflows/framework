using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedInitializedCompositeStateBuilderElsePayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseDataTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedInitializedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
