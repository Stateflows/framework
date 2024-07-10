using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddDefaultTransition<TDefaultTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TDefaultTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition<TDefaultTransition>(State<TTargetState>.Name);

        public static ICompositeStateBuilder AddDefaultTransition<TDefaultTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TDefaultTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TDefaultTransition>(targetVertexName);

        public static ICompositeStateBuilder AddDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
