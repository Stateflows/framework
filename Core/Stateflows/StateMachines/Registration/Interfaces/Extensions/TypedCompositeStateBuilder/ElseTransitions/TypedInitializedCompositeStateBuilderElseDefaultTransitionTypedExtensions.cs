using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ITypedInitializedCompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ITypedInitializedCompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static ITypedInitializedCompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ITypedInitializedCompositeStateBuilder builder, ElseTransitionBuildAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
