using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderInternalTransitionTypedExtensions
    {
        public static ICompositeStateInitialBuilder AddInternalTransition<TEvent, TTransition>(this ICompositeStateInitialBuilder builder)
            where TEvent : Event
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
