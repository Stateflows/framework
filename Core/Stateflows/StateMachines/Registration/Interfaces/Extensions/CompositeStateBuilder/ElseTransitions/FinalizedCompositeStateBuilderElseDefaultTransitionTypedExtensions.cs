using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetVertexName) as IFinalizedCompositeStateBuilder;

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
