using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateInitialBuilderDefaultTransitionTypedExtensions
    {
        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ITypedCompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTransition>(this ITypedCompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static ITypedCompositeStateBuilder AddDefaultTransition<TTargetState>(this ITypedCompositeStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
