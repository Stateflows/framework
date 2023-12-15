using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class FinalizedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this IFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
