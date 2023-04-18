using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateInitialBuilderDefaultTransitionTypedExtensions
    {
        public static ICompositeStateInitialBuilder AddDefaultTransition<TTransition, TTargetState>(this ICompositeStateInitialBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateInitialBuilder AddDefaultTransition<TTransition>(this ICompositeStateInitialBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static ICompositeStateInitialBuilder AddDefaultTransition<TTargetState>(this ICompositeStateInitialBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
