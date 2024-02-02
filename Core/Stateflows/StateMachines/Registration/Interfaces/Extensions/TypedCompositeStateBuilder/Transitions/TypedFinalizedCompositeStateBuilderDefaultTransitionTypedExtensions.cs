using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, TransitionBuildAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
