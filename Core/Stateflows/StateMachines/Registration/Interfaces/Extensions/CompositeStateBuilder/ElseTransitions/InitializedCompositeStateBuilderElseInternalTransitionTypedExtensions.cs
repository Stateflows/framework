using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this IInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
