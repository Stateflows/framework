using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedFinalizedCompositeStateBuilderPayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddDataTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedFinalizedCompositeStateBuilder AddInternalDataTransition<TEventPayload>(this ITypedFinalizedCompositeStateBuilder builder, InternalTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
