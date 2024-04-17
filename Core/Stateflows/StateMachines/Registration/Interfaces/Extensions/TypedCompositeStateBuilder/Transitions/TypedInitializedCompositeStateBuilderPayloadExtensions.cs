using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedInitializedCompositeStateBuilderPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddDataTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedInitializedCompositeStateBuilder AddInternalDataTransition<TEventPayload>(this ITypedInitializedCompositeStateBuilder builder, InternalTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
