using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class FinalizedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
