using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class InitializedCompositeStateBuilderPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IInitializedCompositeStateBuilder AddInternalTransition<TEventPayload>(this IInitializedCompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
