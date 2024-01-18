using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderElseInternalTransitionTypedExtensions
    {
        public static IStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this IStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
