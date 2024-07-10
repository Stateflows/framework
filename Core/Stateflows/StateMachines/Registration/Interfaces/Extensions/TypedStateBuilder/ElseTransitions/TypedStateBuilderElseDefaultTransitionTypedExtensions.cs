using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static ITypedStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ITypedStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
