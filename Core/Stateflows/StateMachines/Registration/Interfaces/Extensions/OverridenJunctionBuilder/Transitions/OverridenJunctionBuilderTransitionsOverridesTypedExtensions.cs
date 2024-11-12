using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenJunctionBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenJunctionBuilder UseTransition<TTargetState>(this IOverridenJunctionBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}