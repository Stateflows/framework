using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderInternalTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddInternalTransition<TEvent, TTransition>(this IFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : Transition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
