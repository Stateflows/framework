using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class StateBuilderPayloadExtensions
    {
        public static IStateBuilder AddTransition<TEventPayload>(this IStateBuilder builder, string targetVertexName, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static IStateBuilder AddInternalTransition<TEventPayload>(this IStateBuilder builder, TransitionBuilderAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
