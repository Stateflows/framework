using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedFinalizedCompositeStateBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload, TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
