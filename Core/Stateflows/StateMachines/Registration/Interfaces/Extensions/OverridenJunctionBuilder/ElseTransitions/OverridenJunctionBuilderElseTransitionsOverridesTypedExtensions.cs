using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenJunctionBuilderElseTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static void UseElseTransition<TTargetState>(this IOverridenJunctionBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => builder.UseElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}