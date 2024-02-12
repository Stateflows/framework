using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
