using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedCompositeStateBuilderPayloadExtensions
    {
        public static ITypedCompositeStateBuilder AddTransition<TEventPayload>(this ITypedCompositeStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedCompositeStateBuilder AddInternalTransition<TEventPayload>(this ITypedCompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
