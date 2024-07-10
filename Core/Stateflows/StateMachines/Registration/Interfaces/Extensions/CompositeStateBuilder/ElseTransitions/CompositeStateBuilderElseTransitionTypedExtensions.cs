using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderElseTransitionTypedExtensions
    {
        public static ICompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this ICompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IBaseState
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        public static ICompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this ICompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetVertexName) as ICompositeStateBuilder;

        public static ICompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this ICompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IBaseState
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
