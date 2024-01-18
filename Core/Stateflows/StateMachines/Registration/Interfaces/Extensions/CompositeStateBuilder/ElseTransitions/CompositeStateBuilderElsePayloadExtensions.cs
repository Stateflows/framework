using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class CompositeStateBuilderElsePayloadExtensions
    {
        public static ICompositeStateBuilder AddElseTransition<TEventPayload>(this ICompositeStateBuilder builder, string targetVertexName, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ICompositeStateBuilder AddElseInternalTransition<TEventPayload>(this ICompositeStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
