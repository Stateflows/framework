using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class StateBuilderElsePayloadExtensions
    {
        public static IStateBuilder AddElseTransition<TEventPayload>(this IStateBuilder builder, string targetVertexName, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IStateBuilder AddElseInternalTransition<TEventPayload>(this IStateBuilder builder, ElseTransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
