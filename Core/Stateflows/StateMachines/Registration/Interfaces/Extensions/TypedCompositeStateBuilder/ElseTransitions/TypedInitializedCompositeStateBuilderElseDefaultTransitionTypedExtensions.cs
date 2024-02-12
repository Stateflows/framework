using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
