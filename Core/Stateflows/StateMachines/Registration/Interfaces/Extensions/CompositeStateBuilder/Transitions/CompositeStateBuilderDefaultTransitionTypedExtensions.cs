using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class CompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TTransition : Transition<Completion>
            where TTargetState : State
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddDefaultTransition<TTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<Completion>
            => builder.AddTransition<Completion, TTransition>(targetVertexName);

        public static ICompositeStateBuilder AddDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, TransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
