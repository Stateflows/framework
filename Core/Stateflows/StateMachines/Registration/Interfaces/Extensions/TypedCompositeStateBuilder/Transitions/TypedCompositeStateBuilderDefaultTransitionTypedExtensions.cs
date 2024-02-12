using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, TransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
