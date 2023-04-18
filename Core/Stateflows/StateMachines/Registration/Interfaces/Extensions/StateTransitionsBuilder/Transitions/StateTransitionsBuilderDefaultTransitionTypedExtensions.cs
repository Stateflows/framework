using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateTransitionsBuilderDefaultTransitionTypedExtensions
    {
        public static IStateTransitionsBuilder AddDefaultTransition<TTransition, TTargetState>(this IStateTransitionsBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IStateTransitionsBuilder AddDefaultTransition<TTransition>(this IStateTransitionsBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static IStateTransitionsBuilder AddDefaultTransition<TTargetState>(this IStateTransitionsBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
