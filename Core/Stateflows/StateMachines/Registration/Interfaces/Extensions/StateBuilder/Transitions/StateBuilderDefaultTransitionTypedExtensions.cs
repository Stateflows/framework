using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderDefaultTransitionTypedExtensions
    {
        public static IStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IStateBuilder AddDefaultTransition<TTransition>(this IStateBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static IStateBuilder AddDefaultTransition<TTargetState>(this IStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
