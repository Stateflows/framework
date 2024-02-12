using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TElseTransition : ElseTransition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition<TElseTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : ElseTransition<CompletionEvent>
            => builder.AddElseTransition<CompletionEvent, TElseTransition>(targetVertexName);

        public static ICompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, ElseTransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddElseDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
