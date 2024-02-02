using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, TransitionBuildAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
