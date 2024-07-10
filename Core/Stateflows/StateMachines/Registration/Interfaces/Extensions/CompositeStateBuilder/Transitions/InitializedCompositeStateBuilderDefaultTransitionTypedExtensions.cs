using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
