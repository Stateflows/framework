using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
