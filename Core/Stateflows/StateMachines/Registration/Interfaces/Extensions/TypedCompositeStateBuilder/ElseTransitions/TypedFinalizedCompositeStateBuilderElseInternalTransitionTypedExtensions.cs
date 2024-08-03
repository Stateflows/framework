using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedFinalizedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
