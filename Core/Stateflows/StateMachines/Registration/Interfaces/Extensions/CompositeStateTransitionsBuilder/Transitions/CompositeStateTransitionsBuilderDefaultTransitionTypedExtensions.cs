using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateTransitionsBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedCompositeStateBuilder builder, string targetStateName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetStateName);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
