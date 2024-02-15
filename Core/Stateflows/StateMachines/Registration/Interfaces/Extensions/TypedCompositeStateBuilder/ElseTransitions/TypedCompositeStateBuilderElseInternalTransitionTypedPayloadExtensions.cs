using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedCompositeStateInitialBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static ITypedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload, TElseTransition>(this ITypedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
