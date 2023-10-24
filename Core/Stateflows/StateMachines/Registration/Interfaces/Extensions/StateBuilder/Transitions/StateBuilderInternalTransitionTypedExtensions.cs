using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderInternalTransitionTypedExtensions
    {
        public static IStateBuilder AddInternalTransition<TEvent, TTransition>(this IStateBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
