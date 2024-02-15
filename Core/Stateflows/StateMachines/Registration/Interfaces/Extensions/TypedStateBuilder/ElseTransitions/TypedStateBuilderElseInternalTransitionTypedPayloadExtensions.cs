using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedStateBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static ITypedStateBuilder AddElseInternalDataTransition<TEventPayload, TElseTransition>(this ITypedStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
