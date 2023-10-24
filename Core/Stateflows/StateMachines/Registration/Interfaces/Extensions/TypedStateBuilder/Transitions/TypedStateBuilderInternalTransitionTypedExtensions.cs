using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedStateBuilderInternalTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddInternalTransition<TEvent, TTransition>(this ITypedStateBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
