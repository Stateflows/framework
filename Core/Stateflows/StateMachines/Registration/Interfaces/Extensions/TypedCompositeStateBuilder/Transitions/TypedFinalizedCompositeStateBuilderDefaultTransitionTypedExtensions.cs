using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedFinalizedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
