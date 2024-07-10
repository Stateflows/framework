using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
