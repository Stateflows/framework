using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderInternalTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddInternalTransition<TEvent, TTransition>(this ITypedFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
