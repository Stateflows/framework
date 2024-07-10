using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static IStateBuilder AddElseDefaultTransition<TElseTransition>(this IStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent>(targetVertexName);

        public static IStateBuilder AddElseDefaultTransition<TTargetState>(this IStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
