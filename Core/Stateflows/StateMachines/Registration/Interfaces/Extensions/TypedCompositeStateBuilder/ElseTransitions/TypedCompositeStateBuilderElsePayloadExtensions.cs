using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedCompositeStateBuilderElsePayloadExtensions
    {
        public static ITypedCompositeStateBuilder AddElseDataTransition<TEventPayload>(this ITypedCompositeStateBuilder builder, string targetVertexName, ElseTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload>(this ITypedCompositeStateBuilder builder, ElseInternalTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddElseInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
