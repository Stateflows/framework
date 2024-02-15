using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class FinalizedCompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static IFinalizedCompositeStateBuilder AddInternalDataTransition<TEventPayload, TTransition>(this IFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}
