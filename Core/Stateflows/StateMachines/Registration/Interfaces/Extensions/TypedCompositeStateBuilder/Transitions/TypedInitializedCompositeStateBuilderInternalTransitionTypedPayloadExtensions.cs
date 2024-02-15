using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedInitializedCompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddInternalDataTransition<TEventPayload, TTransition>(this ITypedInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}
