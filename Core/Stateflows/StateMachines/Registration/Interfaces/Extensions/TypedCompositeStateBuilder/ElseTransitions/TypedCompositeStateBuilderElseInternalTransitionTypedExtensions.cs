using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ITypedCompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
