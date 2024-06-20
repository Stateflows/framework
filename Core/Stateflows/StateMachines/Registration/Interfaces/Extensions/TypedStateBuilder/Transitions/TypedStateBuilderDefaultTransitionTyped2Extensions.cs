using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderDefaultTransitionTyped2Extensions
    {
        public static ITypedStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : class, IState
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static ITypedStateBuilder AddDefaultTransition<TTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedStateBuilder AddDefaultTransition<TTargetState>(this ITypedStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IState
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
