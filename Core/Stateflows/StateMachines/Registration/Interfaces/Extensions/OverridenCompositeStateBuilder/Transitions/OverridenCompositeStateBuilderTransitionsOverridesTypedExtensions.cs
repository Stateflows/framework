using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenCompositeStateBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseTransition<TEvent, TTargetState>(this IOverridenCompositeStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder UseDefaultTransition<TTargetState>(this IOverridenCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}