using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
