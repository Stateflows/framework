using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : State
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static IFinalizedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
