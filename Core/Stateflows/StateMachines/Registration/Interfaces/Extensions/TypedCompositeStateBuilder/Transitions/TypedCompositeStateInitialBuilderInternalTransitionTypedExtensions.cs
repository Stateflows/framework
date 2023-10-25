using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateInitialBuilderInternalTransitionTypedExtensions
    {
        public static ITypedCompositeStateInitialBuilder AddInternalTransition<TEvent, TTransition>(this ITypedCompositeStateInitialBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
