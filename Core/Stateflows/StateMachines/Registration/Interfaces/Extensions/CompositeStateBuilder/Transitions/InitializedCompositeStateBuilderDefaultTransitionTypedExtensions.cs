using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
