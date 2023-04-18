using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateTransitionsBuilderInternalTransitionTypedExtensions
    {
        public static ICompositeStateTransitionsBuilder AddInternalTransition<TEvent, TTransition>(this ICompositeStateTransitionsBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
