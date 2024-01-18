using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ICompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
