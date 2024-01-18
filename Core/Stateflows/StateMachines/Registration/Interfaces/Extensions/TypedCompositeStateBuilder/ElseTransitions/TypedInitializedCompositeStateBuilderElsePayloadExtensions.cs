using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedInitializedCompositeStateBuilderElsePayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedInitializedCompositeStateBuilder AddElseInternalTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
