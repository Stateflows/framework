using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseDefaultTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition<TElseTransition>(State<TTargetState>.Name);

        public static ICompositeStateBuilder AddElseDefaultTransition<TElseTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TElseTransition : class, ITransitionEffect<CompletionEvent>
            => (builder as IStateBuilder).AddElseDefaultTransition<TElseTransition>(targetVertexName) as ICompositeStateBuilder;

        public static ICompositeStateBuilder AddElseDefaultTransition<TTargetState>(this ICompositeStateBuilder builder, ElseDefaultTransitionBuildAction transitionBuildAction = null)
            where TTargetState : class, IBaseState
            => builder.AddElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
