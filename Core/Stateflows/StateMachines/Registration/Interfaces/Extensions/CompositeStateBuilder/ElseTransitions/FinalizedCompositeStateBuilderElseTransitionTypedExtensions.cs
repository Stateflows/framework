using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class FinalizedCompositeStateBuilderElseTransitionTypedExtensions
    {
        public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition, TTargetState>(this IFinalizedCompositeStateBuilder builder)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            where TTargetState : class, IBaseState
            => AddElseTransition<TEvent, TElseTransition>(builder, State<TTargetState>.Name);

        public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TElseTransition>(this IFinalizedCompositeStateBuilder builder, string targetVertexName)
            where TEvent : Event, new()
            where TElseTransition : class, ITransitionEffect<TEvent>
            => (builder as IStateBuilder).AddElseTransition<TEvent, TElseTransition>(targetVertexName) as IFinalizedCompositeStateBuilder;

        public static IFinalizedCompositeStateBuilder AddElseTransition<TEvent, TTargetState>(this IFinalizedCompositeStateBuilder builder, ElseTransitionBuildAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            where TTargetState : class, IBaseState
            => builder.AddElseTransition(State<TTargetState>.Name, transitionBuildAction);
    }
}
