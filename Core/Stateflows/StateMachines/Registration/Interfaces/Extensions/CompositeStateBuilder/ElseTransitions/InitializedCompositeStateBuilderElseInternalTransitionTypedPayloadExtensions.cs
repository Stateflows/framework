using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class InitializedCompositeStateBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseInternalTransition<TEventPayload, TElseTransition>(this IInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
