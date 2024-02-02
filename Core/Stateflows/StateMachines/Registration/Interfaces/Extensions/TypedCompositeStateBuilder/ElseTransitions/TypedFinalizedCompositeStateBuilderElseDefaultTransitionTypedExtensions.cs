using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedFinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static ITypedFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
