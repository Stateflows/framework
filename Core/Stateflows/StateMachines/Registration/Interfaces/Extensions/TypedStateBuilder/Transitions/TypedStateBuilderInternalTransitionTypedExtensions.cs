using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderInternalTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddInternalTransition<TEvent, TTransition>(this ITypedStateBuilder builder)
            where TEvent : Event, new()
            where TTransition : class, IBaseTransition<TEvent>
            => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget);
    }
}
