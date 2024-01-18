using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedInitializedCompositeStateBuilderPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedInitializedCompositeStateBuilder AddInternalTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
