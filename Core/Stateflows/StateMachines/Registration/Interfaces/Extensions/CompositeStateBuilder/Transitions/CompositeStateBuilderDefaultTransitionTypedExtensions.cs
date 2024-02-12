using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddDefaultTransition<TTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TTransition : Transition<CompletionEvent>
            where TTargetState : BaseState
            => builder.AddDefaultTransition<TTransition>(StateInfo<TTargetState>.Name);

        public static ICompositeStateBuilder AddDefaultTransition<TTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TTransition : Transition<CompletionEvent>
            => builder.AddTransition<CompletionEvent, TTransition>(targetVertexName);

        public static ICompositeStateBuilder AddDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, TransitionBuildAction<CompletionEvent> transitionBuildAction = null)
            where TTargetState : BaseState
            => builder.AddDefaultTransition(StateInfo<TTargetState>.Name, transitionBuildAction);
    }
}
