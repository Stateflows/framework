using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Data
{
    public static class TypedStateBuilderPayloadExtensions
    {
        public static ITypedStateBuilder AddDataTransition<TEventPayload>(this ITypedStateBuilder builder, string targetVertexName, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddTransition<Event<TEventPayload>>(targetVertexName, transitionBuildAction);

        public static ITypedStateBuilder AddInternalDataTransition<TEventPayload>(this ITypedStateBuilder builder, TransitionBuildAction<Event<TEventPayload>> transitionBuildAction = null)
            => builder.AddInternalTransition<Event<TEventPayload>>(transitionBuildAction);
    }
}
