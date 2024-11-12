using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenStateBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenStateBuilder UseTransition<TEvent, TTargetState>(this IOverridenStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IOverridenStateBuilder UseDefaultTransition<TTargetState>(this IOverridenStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}