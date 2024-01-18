using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class CompositeStateBuilderPayloadExtensions
    {
        public static ICompositeStateBuilder AddTransition<TEventPayload>(this ICompositeStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ICompositeStateBuilder AddInternalTransition<TEventPayload>(this ICompositeStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
