using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateInitialBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateInitialBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedCompositeStateInitialBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateInitialBuilder AddDefaultTransition<TTransition>(this ITypedCompositeStateInitialBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static ITypedCompositeStateInitialBuilder AddDefaultTransition<TTargetState>(this ITypedCompositeStateInitialBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
