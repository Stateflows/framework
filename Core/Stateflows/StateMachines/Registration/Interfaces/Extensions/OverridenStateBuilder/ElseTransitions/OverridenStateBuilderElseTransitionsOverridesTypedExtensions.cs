using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenStateBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenStateBuilder UseElseTransition<TEvent, TTargetState>(this IOverridenStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IOverridenStateBuilder UseElseDefaultTransition<TTargetState>(this IOverridenStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}