using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenCompositeStateBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseElseTransition<TEvent, TTargetState>(this IOverridenCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseElseDefaultTransition<TTargetState>(this IOverridenCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}