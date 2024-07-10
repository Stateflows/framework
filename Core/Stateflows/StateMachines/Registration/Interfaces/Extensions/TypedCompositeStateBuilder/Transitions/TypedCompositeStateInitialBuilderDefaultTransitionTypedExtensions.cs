using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateInitialBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
