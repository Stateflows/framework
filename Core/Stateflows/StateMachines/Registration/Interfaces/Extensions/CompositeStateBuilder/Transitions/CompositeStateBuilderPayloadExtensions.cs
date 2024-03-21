using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class CompositeStateBuilderPayloadExtensions
    {
        public static ICompositeStateBuilder AddDataTransition<TEventPayload>(this ICompositeStateBuilder builder, string targetVertexName, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ICompositeStateBuilder AddInternalDataTransition<TEventPayload>(this ICompositeStateBuilder builder, InternalTransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
