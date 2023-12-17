using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this IInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : State
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this IInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static IInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this IInitializedCompositeStateBuilder builder, ElseTransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
