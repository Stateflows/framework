using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class StateBuilderElsePayloadExtensions
    {
        public static IStateBuilder AddElseDataTransition<TEventPayload>(this IStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IStateBuilder AddElseInternalDataTransition<TEventPayload>(this IStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
