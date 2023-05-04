using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedStateBuilder AddDefaultTransition<TTransition>(this ITypedStateBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static ITypedStateBuilder AddDefaultTransition<TTargetState>(this ITypedStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
