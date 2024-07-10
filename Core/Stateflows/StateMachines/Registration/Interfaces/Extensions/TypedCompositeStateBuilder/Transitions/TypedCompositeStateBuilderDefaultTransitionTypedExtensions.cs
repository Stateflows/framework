using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ITypedInitializedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IVertex
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
