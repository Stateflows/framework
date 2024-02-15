using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedFinalizedCompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddInternalDataTransition<TEventPayload, TTransition>(this ITypedFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}
