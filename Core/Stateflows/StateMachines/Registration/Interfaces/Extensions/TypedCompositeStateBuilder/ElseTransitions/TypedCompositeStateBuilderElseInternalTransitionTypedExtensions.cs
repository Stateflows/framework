using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : ElseTransition<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
