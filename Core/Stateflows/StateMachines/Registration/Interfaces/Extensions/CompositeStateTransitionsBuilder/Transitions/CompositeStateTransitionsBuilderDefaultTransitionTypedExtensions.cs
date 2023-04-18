using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateTransitionsBuilderDefaultTransitionTypedExtensions
    {
        public static ICompositeStateTransitionsBuilder AddDefaultTransition<TTransition, TTargetState>(this ICompositeStateTransitionsBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateTransitionsBuilder AddDefaultTransition<TTransition>(this ICompositeStateTransitionsBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static ICompositeStateTransitionsBuilder AddDefaultTransition<TTargetState>(this ICompositeStateTransitionsBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
