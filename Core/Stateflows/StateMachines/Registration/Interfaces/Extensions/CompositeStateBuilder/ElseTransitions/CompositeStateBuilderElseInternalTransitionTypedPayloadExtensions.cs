using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class CompositeStateInitialBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static ICompositeStateBuilder AddElseInternalTransition<TEventPayload, TElseTransition>(this ICompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
