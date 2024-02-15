using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class InitializedCompositeStateBuilderElsePayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseDataTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IInitializedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
