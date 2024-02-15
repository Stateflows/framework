using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed.Data
{
    public static class TypedInitializedCompositeStateBuilderElseInternalTransitionTypedPayloadExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseInternalDataTransition<TEventPayload, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Event<TEventPayload>>
            => builder.AddElseTransition<Event<TEventPayload>, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
