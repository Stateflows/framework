using System.Diagnostics;
using Stateflows.Common;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseInternalTransitionTypedExtensions
    {
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddElseInternalTransition<TEvent, TElseTransition>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<TEvent>
            => builder.AddElseTransition<TEvent, TElseTransition>(Constants.DefaultTransitionTarget);
    }
}
