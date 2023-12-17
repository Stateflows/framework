using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateInitialBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static ITypedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, ElseTransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
