using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TElseTransition : ElseTransition<Completion>
            where TTargetState : State
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<Completion>
            => builder.AddElseTransition<Completion, TElseTransition>(targetVertexName);

        public static ICompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, ElseTransitionBuilderAction<Completion> transitionBuildAction = null)
            where TTargetState : State
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
