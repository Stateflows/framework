using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenFinalizedCompositeStateBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IFinalizedOverridenCompositeStateBuilder UseElseTransition<TEvent, TTargetState>(this IFinalizedOverridenCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IFinalizedOverridenCompositeStateBuilder UseElseDefaultTransition<TTargetState>(this IFinalizedOverridenCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}