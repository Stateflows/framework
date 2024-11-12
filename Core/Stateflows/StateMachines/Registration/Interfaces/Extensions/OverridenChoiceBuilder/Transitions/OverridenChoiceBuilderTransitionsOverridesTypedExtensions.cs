using System.Diagnostics;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class OverridenChoiceBuilderTransitionsOverridesTypedExtensions
    {
        [DebuggerHidden]
        public static IOverridenChoiceBuilder UseTransition<TTargetState>(this IOverridenChoiceBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.UseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}