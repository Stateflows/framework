using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenBehaviorStateBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder UseTransition<TEvent, TTargetState>(this IBehaviorOverridenStateBuilder builder, TransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
        
        [DebuggerHidden]
        public static IBehaviorOverridenStateBuilder UseDefaultTransition<TTargetState>(this IBehaviorOverridenStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}