using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class FinalizedCompositeStateBuilderElsePayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IFinalizedCompositeStateBuilder AddElseInternalTransition<TEventPayload>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
