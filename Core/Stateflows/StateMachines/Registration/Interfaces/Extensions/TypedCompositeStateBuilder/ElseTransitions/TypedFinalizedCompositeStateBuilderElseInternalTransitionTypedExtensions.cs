using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedFinalizedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
