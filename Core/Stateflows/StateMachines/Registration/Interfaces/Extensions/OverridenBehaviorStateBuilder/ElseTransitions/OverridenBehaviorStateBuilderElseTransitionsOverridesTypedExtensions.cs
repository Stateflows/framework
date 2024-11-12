using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenBehaviorStateBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder UseElseTransition<TEvent, TTargetState>(this IBehaviorOverridenStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder UseElseDefaultTransition<TTargetState>(this IBehaviorOverridenStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}