using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TTransition : class, IBaseTransition<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition<TTransition>(State<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : class, IBaseTransition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static IFinalizedCompositeStateBuilder AddDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, DefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
