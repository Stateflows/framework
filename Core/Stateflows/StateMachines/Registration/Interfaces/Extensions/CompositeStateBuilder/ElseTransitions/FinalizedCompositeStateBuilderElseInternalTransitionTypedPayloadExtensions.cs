using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class FinalizedCompositeStateBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload, TElseTransition>(this IFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
