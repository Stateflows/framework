using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
