using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenChoiceBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static void UseElseTransition<TTargetState>(this IOverridenChoiceBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}