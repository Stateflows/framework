using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetVertexName) as IInitializedCompositeStateBuilder;

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
