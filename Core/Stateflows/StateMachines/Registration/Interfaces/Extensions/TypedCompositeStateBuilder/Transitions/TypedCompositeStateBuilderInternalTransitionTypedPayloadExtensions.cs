using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedCompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static ITypedCompositeStateBuilder AddInternalTransition<TEventPayload, TTransition>(this ITypedCompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}
