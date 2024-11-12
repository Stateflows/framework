using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenFinalizedCompositeStateBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IFinalizedOverridenCompositeStateBuilder UseTransition<TEvent, TTargetState>(this IFinalizedOverridenCompositeStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IFinalizedOverridenCompositeStateBuilder UseDefaultTransition<TTargetState>(this IFinalizedOverridenCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}