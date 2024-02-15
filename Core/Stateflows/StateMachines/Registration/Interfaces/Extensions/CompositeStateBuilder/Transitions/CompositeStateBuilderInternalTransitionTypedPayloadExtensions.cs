using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class CompositeStateBuilderInternalTransitionTypedPayloadExtensions
    {
        public static ICompositeStateBuilder AddInternalDataTransition<TEventPayload, TTransition>(this ICompositeStateBuilder builder)
            where TTransition : Transition<Event<TEventPayload>>
            => builder.AddTransition<Event<TEventPayload>, TTransition>(Constants.DefaultTransitionTarget);
    }
}
