using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderDefaultTransitionTypedExtensions
    {
        public static IStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IStateBuilder builder)
            where TTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static IStateBuilder AddDefaultTransition<TTransition>(this IStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IStateBuilder AddDefaultTransition<TTargetState>(this IStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
